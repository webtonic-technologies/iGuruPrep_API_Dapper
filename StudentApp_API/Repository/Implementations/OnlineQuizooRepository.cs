using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Models;
using System.Data;
using System.Threading.Channels;
using System.Collections.Generic;

namespace StudentApp_API.Repository.Implementations
{
    public class OnlineQuizooRepository: IOnlineQuizooRepository
    {
        private readonly IDbConnection _connection;

        public OnlineQuizooRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> InsertQuizooAsync(OnlineQuizooDTO quizoo)
        {
            try
            {
                // Step 1: Check for an ongoing quiz for the same user
                var ongoingQuiz = await _connection.QueryFirstOrDefaultAsync<DateTime?>(
                    @"SELECT TOP 1 QuizooStartTime
              FROM tblQuizoo
              WHERE CreatedBy = @CreatedBy 
                AND IsSystemGenerated = 1
                AND DATEADD(MINUTE, CAST(SUBSTRING(Duration, 1, CHARINDEX(' ', Duration) - 1) AS INT), QuizooStartTime) > GETDATE()
              ORDER BY QuizooStartTime DESC",
                    new { quizoo.CreatedBy });

                if (ongoingQuiz.HasValue)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false,
                        $"You cannot create a new quiz while an existing quiz is ongoing. Current ongoing quiz started at {ongoingQuiz.Value}.",
                        new List<QuestionResponseDTO>(), 400);
                }

                // Step 2: Validate against existing quizzes for the same user (15-minute gap)
                var conflictingQuiz = await _connection.QueryFirstOrDefaultAsync<DateTime?>(
                    @"SELECT TOP 1 QuizooStartTime
              FROM tblQuizoo
              WHERE CreatedBy = @CreatedBy 
                AND ABS(DATEDIFF(MINUTE, QuizooStartTime, @QuizooStartTime)) < 15",
                    new { quizoo.CreatedBy, QuizooStartTime = DateTime.Now });

                if (conflictingQuiz.HasValue)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false,
                        $"A quiz is already scheduled at {conflictingQuiz.Value}. Ensure at least a 15-minute gap between quizzes.",
                        new List<QuestionResponseDTO>(), 400);
                }

                // Step 3: Get Student Mapping (Board, Class, Course)
                string queryMapping = @"
            SELECT TOP 1 [BoardId], [ClassID], [CourseID]
            FROM [tblStudentClassCourseMapping]
            WHERE [RegistrationID] = @RegistrationId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync<StudentMappingDTO>(
                    queryMapping, new { RegistrationId = quizoo.CreatedBy });

                if (studentMapping == null)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false,
                        "Student mapping not found.", new List<QuestionResponseDTO>(), 404);
                }

                int quizooId;
                var quizooDto = new
                {
                    QuizooDate = DateTime.Now,
                    QuizooStartTime = DateTime.Now,
                    Duration = "30 min",
                    NoOfQuestions = 30,
                    NoOfPlayers = 10,
                    CreatedBy = quizoo.CreatedBy,
                    IsSystemGenerated = true,
                    ClassID = studentMapping.ClassID,
                    CourseID = studentMapping.CourseID,
                    BoardID = studentMapping.BoardId
                };

                // Step 4: Insert `tblQuizoo`
                var insertQuery = @"
            INSERT INTO tblQuizoo (
                QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                NoOfPlayers, CreatedBy, IsSystemGenerated, CreatedOn,
                ClassID, CourseID, BoardID
            ) VALUES (
                @QuizooDate, @QuizooStartTime, @Duration, @NoOfQuestions, 
                @NoOfPlayers, @CreatedBy, @IsSystemGenerated, GETDATE(),
                @ClassID, @CourseID, @BoardID
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int)";

                quizooId = await _connection.ExecuteScalarAsync<int>(insertQuery, quizooDto);

                // Step 5: Get Quiz Questions
                var response = await GetQuizQuestions(studentMapping.BoardId, studentMapping.ClassID, studentMapping.CourseID, quizooId);

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Quizoo inserted/updated successfully.", response.Data, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, $"Error: {ex.Message}", new List<QuestionResponseDTO>(), 500);
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
        mc.Answermultiplechoicecategoryid as MultiAnswerid,
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
            // Check if the given userId exists in tblRegistration
            string checkUserQuery = "SELECT COUNT(*) FROM tblRegistration WHERE RegistrationID = @UserID";
            int userCount = await _connection.ExecuteScalarAsync<int>(checkUserQuery, new { UserID = userId });
            if (userCount == 0)
            {
                return new ServiceResponse<List<StudentRankDTO>>(false, "User not found in registration.", null, 404);
            }
            // SQL query to fetch student rank list with name, with the passed userId coming first
    //        var query = @"
    //WITH StudentCorrectAnswers AS (
    //    SELECT 
    //        s.StudentID,
    //        COUNT(*) AS CorrectAnswers
    //    FROM 
    //        tblQuizooPlayersAnswers s
    //    WHERE 
    //        s.QuizooID = @QuizooID AND s.IsCorrect = 1
    //    GROUP BY 
    //        s.StudentID
    //),
    //RankedStudents AS (
    //    SELECT 
    //        s.StudentID,
    //        COALESCE(ca.CorrectAnswers, 0) AS CorrectAnswers,
    //        ROW_NUMBER() OVER (ORDER BY 
    //            CASE WHEN s.StudentID = @UserID THEN 0 ELSE ca.CorrectAnswers END DESC
    //        ) AS Rank
    //    FROM 
    //        (SELECT DISTINCT StudentID FROM tblQuizooPlayersAnswers WHERE QuizooID = @QuizooID) s
    //    LEFT JOIN 
    //        StudentCorrectAnswers ca ON s.StudentID = ca.StudentID
    //)
    //SELECT 
    //    rs.StudentID,
    //    rs.CorrectAnswers,
    //    rs.Rank,
    //    r.FirstName,
    //    r.LastName,
    //    c.CountryName as Country
    //FROM 
    //    RankedStudents rs
    //JOIN 
    //    tblRegistration r ON rs.StudentID = r.RegistrationID
    //    Join tblCountries c on r.CountryID = c.CountryId
    //ORDER BY 
    //    rs.Rank;";
            var query = @"WITH StudentCorrectAnswers AS (
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
        ROW_NUMBER() OVER (ORDER BY COALESCE(ca.CorrectAnswers, 0) DESC) AS Rank
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
    r.LastName,
    c.CountryName AS Country
FROM 
    RankedStudents rs
JOIN 
    tblRegistration r ON rs.StudentID = r.RegistrationID
JOIN 
    tblCountries c ON r.CountryID = c.CountryId
ORDER BY 
    CASE WHEN rs.StudentID = @UserID THEN 0 ELSE rs.Rank END;
";
            // Execute the query and map results to a list of StudentRankDTO
            var result = await _connection.QueryAsync<StudentRankDTO>(query, new { QuizooID = quizooId, UserID = userId });
            if (!result.Any())
            {
                return new ServiceResponse<List<StudentRankDTO>>(false, "no records found", [], 404);
            }
            // Return the result as a list
            return new ServiceResponse<List<StudentRankDTO>>(true, "operation successful", result.ToList(), 200);
        }
        public async Task<ServiceResponse<int>> SetForceExitAsync(int QuizooID, int StudentID)
        {
            const string query = "UPDATE tblQuizooOnlinePlayers SET IsForceExit = 1 WHERE QuizooID = @QuizooID and StudentID = @StudentID";
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(query, new { QuizooID, StudentID });

                if (rowsAffected == 0)
                {
                    return new ServiceResponse<int>(false, "Operation failed: No record updated. Check if QPID is valid.", 0, 404);
                }

                return new ServiceResponse<int>(true, "Operation successful", rowsAffected, 200);
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
//            var questions = await _connection.QueryAsync<QuestionResponseDTO>(
//"SELECT TOP(@Limit) q.*, qt.QuestionType AS QuestionTypeName " +
//"FROM tblQuestion q " +
//"INNER JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID " +
//"WHERE q.ContentIndexId IN @ContentIndexIds " +
//"AND q.SubjectID IN @SubjectIds " +
//"AND q.IndexTypeId IN @IndexTypeIds " +  // Apply filter for IndexTypeId (Chapter, Topic, Subtopic)
//"AND q.IsConfigure = 1 AND q.IsLive = 1 " +
//"AND q.QuestionTypeId IN (1, 2, 10, 6)" +
//"AND q.IsRejected = 0 " +
//"ORDER BY NEWID()",  // Random order
//new
//{
//    ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
//    SubjectIds = filteredContent.Select(c => c.SubjectId),
//    IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),  // Pass the IndexTypeId values
//    Limit = 30  // Use the NoOfQuestions limit from tblQuizoo
//});
            var questions = await _connection.QueryAsync<QuestionResponseDTO>(
"SELECT TOP(@Limit) q.*, qt.QuestionType AS QuestionTypeName " +
"FROM tblQuestion q " +
"INNER JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID " +
"WHERE q.ContentIndexId IN @ContentIndexIds " +
"AND q.SubjectID IN @SubjectIds " +
"AND q.IndexTypeId IN @IndexTypeIds " +  // Apply filter for IndexTypeId (Chapter, Topic, Subtopic)
"AND q.IsConfigure = 1 AND q.IsLive = 1 " +
"AND q.QuestionTypeId IN (1, 2, 10, 6)" +
"AND q.IsRejected = 0 " +
"AND EXISTS (SELECT 1 FROM tblQIDCourse qc WHERE qc.QuestionCode = q.QuestionCode AND qc.LevelId = 1 " +
"AND qc.CourseID = @CourseID)" +
"ORDER BY NEWID()",  // Random order
new
{
    ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
    SubjectIds = filteredContent.Select(c => c.SubjectId),
    IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),  // Pass the IndexTypeId values
    Limit = 30,  // Use the NoOfQuestions limit from tblQuizoo
    CourseID = CourseId
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
        private QuestionResponseDTO GetQuestionById(int QuestionId)
        {
            string sql = @"
                SELECT q.*, 
                       c.CourseName, 
                       b.BoardName, 
                       cl.ClassName, 
                       s.SubjectName,
                       et.ExamTypeName,
                       e.EmpFirstName,
                       qt.QuestionType as QuestionTypeName,
                       it.IndexType as IndexTypeName,
                       CASE 
                           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                       END AS ContentIndexName
                FROM tblQuestion q
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE q.QuestionId = @QuestionId AND q.IsActive = 1";

            var parameters = new { QuestionId = QuestionId };

            var item = _connection.QueryFirstOrDefault<dynamic>(sql, parameters);

            if (item != null)
            {
                if (item.QuestionTypeId == 11)
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        Paragraph = item.Paragraph,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        // Qid = GetListOfQIDCourse(item.QuestionCode),
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        ContentIndexId = item.ContentIndexId,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        subjectID = item.subjectID,
                        ModifiedOn = item.ModifiedOn,
                        QuestionTypeId = item.QuestionTypeId,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive,
                        ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                    };
                    return questionResponse;
                }
                else
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        // QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        ContentIndexId = item.ContentIndexId,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        subjectID = item.subjectID,
                        ModifiedOn = item.ModifiedOn,
                        QuestionTypeId = item.QuestionTypeId,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive,
                        MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                        MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                        Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                        AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                    };
                    return questionResponse;
                }

            }
            else
            {
                return null;
            }
        }
        private List<ParagraphQuestions> GetChildQuestions(string QuestionCode)
        {
            string sql = @"
                SELECT q.*, 
                       c.CourseName, 
                       b.BoardName, 
                       cl.ClassName, 
                       s.SubjectName,
                       et.ExamTypeName,
                       e.EmpFirstName,
                       qt.QuestionType as QuestionTypeName,
                       it.IndexType as IndexTypeName,
                       CASE 
                           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                       END AS ContentIndexName
                FROM tblQuestion q
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE q.ParentQCode = @QuestionCode AND q.IsActive = 1 AND IsLive = 0 AND q.IsConfigure = 1";
            var parameters = new { QuestionCode = QuestionCode };
            var item = _connection.Query<dynamic>(sql, parameters);
            var response = item.Select(m => new ParagraphQuestions
            {
                QuestionId = m.QuestionId,
                QuestionDescription = m.QuestionDescription,
                ParentQId = m.ParentQId,
                ParentQCode = m.ParentQCode,
                QuestionTypeId = m.QuestionTypeId,
                Status = m.Status,
                CategoryId = m.CategoryId,
                CreatedBy = m.CreatedBy,
                CreatedOn = m.CreatedOn,
                ModifiedBy = m.ModifiedBy,
                ModifiedOn = m.ModifiedOn,
                subjectID = m.SubjectID,
                EmployeeId = m.EmployeeId,
                ModifierId = m.ModifierId,
                IndexTypeId = m.IndexTypeId,
                ContentIndexId = m.ContentIndexId,
                IsRejected = m.IsRejected,
                IsApproved = m.IsApproved,
                QuestionCode = m.QuestionCode,
                Explanation = m.Explanation,
                ExtraInformation = m.ExtraInformation,
                IsActive = m.IsActive,
                IsConfigure = m.IsConfigure,
                AnswerMultipleChoiceCategories = GetMultipleAnswers(m.QuestionCode),
                Answersingleanswercategories = GetSingleAnswer(m.QuestionCode, m.QuestionId)
            }).ToList();
            return response;
        }
        private List<MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
        {
            const string getAnswerIdQuery = @"
        SELECT AnswerId 
        FROM tblAnswerMaster
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";

            const string getAnswersQuery = @"
        SELECT MatchThePair2Id, PairColumn, PairRow
        FROM tblOptionsMatchThePair2
        WHERE AnswerId = @AnswerId";


            var answerId = _connection.QueryFirstOrDefault<int?>(getAnswerIdQuery, new { QuestionCode = questionCode, QuestionId = questionId });

            if (answerId == null)
            {
                return new List<MatchThePairAnswer>();
            }

            return _connection.Query<MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();

        }
        private Answersingleanswercategory GetSingleAnswer(string QuestionCode, int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<StudentApp_API.Models.AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode and Questionid = @Questionid", new { QuestionCode, Questionid = QuestionId });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswersingleanswercategory] WHERE [Answerid] = @Answerid";

                var response = _connection.QueryFirstOrDefault<Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new Answersingleanswercategory();
            }
            else
            {
                return new Answersingleanswercategory();
            }
        }
    }
}
public class StudentMappingDTO
{
    public int BoardId { get; set; }
    public int ClassID { get; set; }
    public int CourseID { get; set; }
}
