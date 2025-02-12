using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Models;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class OnlineQuizooRepository: IOnlineQuizooRepository
    {
        private readonly IDbConnection _connection;

        public OnlineQuizooRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> InsertQuizooAsync(QuizooDTO quizoo)
        {
            try
            {
                // Step 1: Validate Quiz Start Time
                if (quizoo.QuizooStartTime <= DateTime.Now.AddMinutes(15))
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "Quiz start time must be at least 15 minutes from the current time.", [], 400);
                }

                // Step 2: Validate against existing quizzes for the same user
                var conflictingQuiz = await _connection.QueryFirstOrDefaultAsync<DateTime?>(
                    @"SELECT TOP 1 QuizooStartTime
              FROM tblQuizoo
              WHERE CreatedBy = @CreatedBy 
                AND ABS(DATEDIFF(MINUTE, QuizooStartTime, @QuizooStartTime)) < 15",
                    new { quizoo.CreatedBy, quizoo.QuizooStartTime });

                if (conflictingQuiz.HasValue)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false,
                        $"A quiz is already scheduled at {conflictingQuiz.Value}. Ensure at least a 15-minute gap between quizzes.",
                        [], 400);
                }

                int quizooId;
                quizoo.IsSystemGenerated = true;

                // Step 3: Insert or Update `tblQuizoo`
               
                    // Insert
                    var insertQuery = @"
                INSERT INTO tblQuizoo (
                    QuizooName, QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                    NoOfPlayers, QuizooLink, CreatedBy, QuizooDuration, IsSystemGenerated, CreatedOn,
                    ClassID, CourseID, BoardID
                ) VALUES (
                    @QuizooName, @QuizooDate, @QuizooStartTime, @Duration, @NoOfQuestions, 
                    @NoOfPlayers, @QuizooLink, @CreatedBy, @QuizooDuration, @IsSystemGenerated, GETDATE()
                    @ClassID, @CourseID, @BoardID
                ); 
                SELECT CAST(SCOPE_IDENTITY() as int)";

                    quizooId = await _connection.ExecuteScalarAsync<int>(insertQuery, quizoo);
                    quizoo.QuizooLink = $"iGuruQuizooLink/{quizooId}";
                    await _connection.ExecuteAsync(@"update tblQuizoo set QuizooLink = @QuizooLink", new { QuizooLink = quizoo.QuizooLink });


                var response = await GetQuizQuestions(quizoo.BoardID, quizoo.ClassID, quizoo.CourseID, quizooId);

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Quizoo inserted/updated successfully.", response.Data, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, $"Error: {ex.Message}", [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionWithCorrectAnswerDTO>>> GetQuestionsWithCorrectAnswersAsync(int quizooId)
        {
            // SQL query to fetch questions with their correct answers
            var query = @"
    SELECT 
        qq.QuizooID,
        qq.QuestionID,
        q.QuestionCode,
        q.QuestionDescription,
        mc.Answermultiplechoicecategoryid,
        mc.Answer
    FROM 
        tblQuizooQuestions qq
    JOIN 
        tblQuestion q ON qq.QuestionID = q.QuestionId
    JOIN 
        tblAnswerMaster am ON q.QuestionId = am.Questionid
    JOIN 
        tblAnswerMultipleChoiceCategory mc ON am.Answerid = mc.Answerid
    WHERE 
        qq.QuizooID = @QuizooID
        AND mc.Iscorrect = 1
    ORDER BY 
        qq.DisplayOrder";

            // Execute the query and map results to a list of QuestionWithCorrectAnswerDTO
            var result = await _connection.QueryAsync<QuestionWithCorrectAnswerDTO>(query, new { QuizooID = quizooId });
            if (!result.Any())
            {
                return new ServiceResponse<List<QuestionWithCorrectAnswerDTO>>(false, "No records found", [], 404);
            }
            // Return the result as a list
            return new ServiceResponse<List<QuestionWithCorrectAnswerDTO>>(true, "operation successful", result.ToList(), 200);
        }
        public async Task<ServiceResponse<List<StudentRankDTO>>> GetStudentRankListAsync(int quizooId, int userId)
        {
            // SQL query to fetch student rank list with name, with the passed userId coming first
            var query = @"
    WITH StudentCorrectAnswers AS (
        SELECT 
            s.StudentID,
            COUNT(*) AS CorrectAnswers
        FROM 
            tblQuizooPlayersAnswers s
        WHERE 
            s.QuizooID = @QuizooID AND s.IsCorrect = 1
        GROUP BY 
            s.StudentID
    ),
    RankedStudents AS (
        SELECT 
            s.StudentID,
            COALESCE(ca.CorrectAnswers, 0) AS CorrectAnswers,
            ROW_NUMBER() OVER (ORDER BY 
                CASE WHEN s.StudentID = @UserID THEN 0 ELSE ca.CorrectAnswers END DESC
            ) AS Rank
        FROM 
            (SELECT DISTINCT StudentID FROM tblQuizooPlayersAnswers WHERE QuizooID = @QuizooID) s
        LEFT JOIN 
            StudentCorrectAnswers ca ON s.StudentID = ca.StudentID
    )
    SELECT 
        rs.StudentID,
        rs.CorrectAnswers,
        rs.Rank,
        r.FirstName,
        r.LastName
    FROM 
        RankedStudents rs
    JOIN 
        tblRegistration r ON rs.StudentID = r.RegistrationID
    ORDER BY 
        rs.Rank;";

            // Execute the query and map results to a list of StudentRankDTO
            var result = await _connection.QueryAsync<StudentRankDTO>(query, new { QuizooID = quizooId, UserID = userId });

            // Return the result as a list
            return new ServiceResponse<List<StudentRankDTO>>(true, "operation successful", result.ToList(), 200);
        }
        public async Task<ServiceResponse<int>> SetForceExitAsync(int qpid)
        {
            const string query = "UPDATE tblQuizooOnlinePlayers SET IsForceExit = 1 WHERE QPID = @QPID";
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(query, new { QPID = qpid });
                return new ServiceResponse<int>(true, "Operation successful", rowsAffected, 200); // Return true if any rows were updated.
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
         SELECT TOP 1 * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode ORDER BY AnswerId DESC", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswerMultipleChoiceCategory] WHERE [Answerid] = @Answerid";

                var response = _connection.Query<AnswerMultipleChoiceCategory>(getQuery, new { answerMaster.Answerid });
                return response.AsList() ?? new List<AnswerMultipleChoiceCategory>();
            }
            else
            {
                return new List<AnswerMultipleChoiceCategory>();
            }
        }
        private async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuizQuestions(int BoardId, int ClassId, int CourseId, int QuizooId)
        {
            _connection.Open();

            // 1. Fetch Syllabus based on Board, Class, and Course
            var syllabus = await _connection.QueryFirstOrDefaultAsync<SyllabusDetails>(
                @"SELECT SyllabusId, BoardID, CourseId, ClassId, SyllabusName
          FROM tblSyllabus
          WHERE BoardID = @BoardId AND CourseId = @CourseId AND ClassId = @ClassId",
                new { BoardId, CourseId, ClassId });

            if (syllabus == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No syllabus found", new List<QuestionResponseDTO>(), 404);

            // 2. Fetch Subjects mapped to the Syllabus
            var subjects = await _connection.QueryAsync<SyllabusSubjectMapping>(
                @"SELECT SubjectID
          FROM tblSyllabusSubjects
          WHERE SyllabusID = @SyllabusId",
                new { SyllabusId = syllabus.SyllabusId });

            if (!subjects.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No subjects found for the syllabus", new List<QuestionResponseDTO>(), 404);

            // 3. Fetch Content Mappings for each Subject
            var syllabusDetails = await _connection.QueryAsync<ContentDetails>(
                @"SELECT ContentIndexId, IndexTypeId, SubjectId
          FROM tblSyllabusDetails
          WHERE SyllabusID = @SyllabusId AND SubjectID IN @SubjectIds",
                new { SyllabusId = syllabus.SyllabusId, SubjectIds = subjects.Select(s => s.SubjectID) });

            if (!syllabusDetails.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No content mappings found for the syllabus subjects", new List<QuestionResponseDTO>(), 404);

            // Filtered content details remain unchanged
            var filteredContent = syllabusDetails.ToList();

            // 4. Fetch questions based on ContentIndexId and IndexTypeId
            var questions = await _connection.QueryAsync<QuestionResponseDTO>(
                @"SELECT TOP(@Limit) *
          FROM tblQuestion
          WHERE ContentIndexId IN @ContentIndexIds
            AND SubjectID IN @SubjectIds
            AND IndexTypeId IN @IndexTypeIds
            AND IsConfigure = 1 AND IsLive = 1 AND QuestionTypeId IN (1, 2, 10, 6)
          ORDER BY NEWID()",
                new
                {
                    ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
                    SubjectIds = filteredContent.Select(c => c.SubjectId),
                    IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),
                    Limit = 30
                });

            if (!questions.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No questions found", new List<QuestionResponseDTO>(), 404);

            // Prepare response
            var response = questions.Select(item =>
            {
                return new QuestionResponseDTO
                {
                    QuestionId = item.QuestionId,
                    QuestionDescription = item.QuestionDescription,
                    QuestionTypeId = item.QuestionTypeId,
                    Status = item.Status,
                    CreatedBy = item.CreatedBy,
                    CreatedOn = item.CreatedOn,
                    ModifiedBy = item.ModifiedBy,
                    ModifiedOn = item.ModifiedOn,
                    subjectID = item.subjectID,
                    SubjectName = item.SubjectName,
                    EmployeeId = item.EmployeeId,
                    IndexTypeId = item.IndexTypeId,
                    IndexTypeName = item.IndexTypeName,
                    ContentIndexId = item.ContentIndexId,
                    ContentIndexName = item.ContentIndexName,
                    IsRejected = item.IsRejected,
                    IsApproved = item.IsApproved,
                    QuestionTypeName = item.QuestionTypeName,
                    QuestionCode = item.QuestionCode,
                    Explanation = item.Explanation,
                    ExtraInformation = item.ExtraInformation,
                    IsActive = item.IsActive,
                    MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                    AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                };
            }).ToList();

            // Insert Quizoo-Question mappings
            var insertQuery = @"
        INSERT INTO tblQuizooQuestions (QuizooID, QuestionID, DisplayOrder)
        VALUES (@QuizooID, @QuestionID, @DisplayOrder)";
            await _connection.ExecuteAsync(insertQuery, questions.Select((q, index) => new
            {
                QuizooID = QuizooId,
                QuestionID = q.QuestionId,
                DisplayOrder = index + 1
            }));

            return response.Any()
                ? new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", response, 200)
                : new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        }
    }
}