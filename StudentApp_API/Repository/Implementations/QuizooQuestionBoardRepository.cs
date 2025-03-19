using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Models;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class QuizooQuestionBoardRepository : IQuizooQuestionBoardRepository
    {


        private readonly IDbConnection _connection;

        public QuizooQuestionBoardRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuizQuestions(int quizooId, int registrationId)
        {
            _connection.Open();

            // 1. Fetch Student Details
            var studentDetails = await _connection.QuerySingleOrDefaultAsync<StudentDetails>(
                "SELECT SCCMID, RegistrationID, CourseID, ClassID, BoardId " +
                "FROM tblStudentClassCourseMapping WHERE RegistrationID = @RegistrationID",
                new { RegistrationID = registrationId });

            if (studentDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // 2. Fetch Syllabus Details based on student course/class/board
            var syllabusDetails = await _connection.QueryAsync<SyllabusDetails>(
                "SELECT SyllabusId, BoardID, CourseId, ClassId, SyllabusName " +
                "FROM tblSyllabus WHERE BoardID = @BoardId AND CourseId = @CourseId AND ClassId = @ClassId",
                new { studentDetails.BoardId, studentDetails.CourseID, studentDetails.ClassID });

            if (!syllabusDetails.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // 3. Fetch Quizoo-Subject-Chapter Mapping
            var quizooSubjects = await _connection.QueryAsync<QuizooSubjectMapping>(
                "SELECT QuizooID, SubjectID, ChapterID " +
                "FROM tblQuizooSyllabus WHERE QuizooID = @QuizooID",
                new { QuizooID = quizooId });

            if (!quizooSubjects.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
            // 4. Fetch Chapters (associated with SubjectId)
            var chapters = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
        CIC.ContentIndexId AS ChapterContentIndexId,
        CIC.SubjectId AS SubjectId,
        CIC.ContentName_Chapter AS ContentName,
        CIC.IndexTypeId AS IndexTypeId
    FROM tblContentIndexChapters CIC
    WHERE CIC.SubjectId IN @SubjectIds",
                new { SubjectIds = quizooSubjects.Select(q => q.SubjectID) }
            );

            // 5. Fetch Topics (associated with ChapterId)
            var topics = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
        CIT.ContInIdTopic AS TopicContentIndexId,
        CIT.ContentIndexId AS ChapterId,
        CIT.ContentName_Topic AS ContentName,
        CIT.IndexTypeId AS IndexTypeId
    FROM tblContentIndexTopics CIT
    WHERE CIT.ContentIndexId IN @ChapterIds",
                new { ChapterIds = chapters.Select(c => c.ChapterContentIndexId) } // Use Chapter IDs for topic association
            );

            // 6. Fetch Subtopics (associated with TopicId)
            var subtopics = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
        CIS.ContInIdSubTopic AS SubTopicContentIndexId,
        CIS.ContInIdTopic AS TopicId,
        CIS.ContentName_SubTopic AS ContentName,
        CIS.IndexTypeId AS IndexTypeId
    FROM tblContentIndexSubTopics CIS
    WHERE CIS.ContInIdTopic IN @TopicIds",
                new { TopicIds = topics.Select(t => t.TopicContentIndexId) } // Use Topic IDs for subtopic association
            );

            // 7. Combine Chapters, Topics, and Subtopics
            var combinedContentDetails = chapters
                .Concat(topics)
                .Concat(subtopics)
                .Select(c => new ContentDetails
                {
                    ContentIndexId = c.ChapterContentIndexId != 0
                        ? c.ChapterContentIndexId
                        : c.TopicContentIndexId != 0
                            ? c.TopicContentIndexId
                            : c.SubTopicContentIndexId,
                    SubjectId = c.SubjectId,
                    ContentName = c.ContentName,
                    IndexTypeId = c.IndexTypeId
                })
                .Where(c => c.ContentIndexId != 0) // Filter out empty content entries
                .Distinct() // Ensure unique records
                .ToList();

            // 8. Fetch existing mappings from tblSyllabusDetails
            // 1. Fetch existing mappings from tblSyllabusDetails
            var syllabusContentMapping = await _connection.QueryAsync(
                @"SELECT DISTINCT ContentIndexId, IndexTypeId 
      FROM tblSyllabusDetails 
      WHERE SubjectId IN @SubjectIds 
      AND ContentIndexId IN @ContentIndexIds 
      AND IndexTypeId IN @IndexTypeIds",
                new
                {
                    SubjectIds = quizooSubjects.Select(q => q.SubjectID),
                    ContentIndexIds = combinedContentDetails.Select(c => c.ContentIndexId),
                    IndexTypeIds = combinedContentDetails.Select(c => c.IndexTypeId)
                });

            // 2. Convert syllabusContentMapping into a simple list of key-value pairs
            var syllabusMappingList = syllabusContentMapping
                .Select(m => new { m.ContentIndexId, m.IndexTypeId })
                .ToList();

            // 3. Filter the combined content details manually using LINQ's `Any` method
            var filteredContent = combinedContentDetails
                .Where(c => syllabusMappingList.Any(m => m.ContentIndexId == c.ContentIndexId && m.IndexTypeId == c.IndexTypeId))
                .ToList();

            if (!filteredContent.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // 7. Fetch the limit for the number of questions from tblQuizoo
            var quizooDetails = await _connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT NoOfQuestions, Duration FROM tblQuizoo WHERE QuizooID = @QuizooID",
                new { QuizooID = quizooId });

            if (quizooDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            int limit = quizooDetails.NoOfQuestions;
            // Split the string and parse the numeric part
            string[] parts = quizooDetails.Duration.Split(' '); // Split by space
            int durationPerQuestion;
            if (parts.Length > 0 && int.TryParse(parts[0], out int duration))
            {
                durationPerQuestion = duration / limit;
                Console.WriteLine($"Duration per question: {durationPerQuestion} minutes");
            }
            else
            {
                throw new Exception("Invalid Duration format. Unable to parse numeric value.");
            }
            // 8. Fetch questions based on ContentIndexId and IndexTypeId
            //var questions = await _connection.QueryAsync<QuestionResponseDTO>(
            //    "SELECT TOP(@Limit) * " +
            //    "FROM tblQuestion " +
            //    "WHERE ContentIndexId IN @ContentIndexIds " +
            //    "AND SubjectID IN @SubjectIds " +
            //    "AND IndexTypeId IN @IndexTypeIds " + // Apply filter for IndexTypeId (Chapter, Topic, Subtopic)
            //    "AND IsConfigure = 1 AND IsLive = 1 AND QuestionTypeId IN (1, 2, 10, 6) " +
            //    "ORDER BY NEWID()", // This orders questions randomly
            //    new
            //    {
            //        ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
            //        SubjectIds = filteredContent.Select(c => c.SubjectId),
            //        IndexTypeIds = filteredContent.Select(c => c.IndexTypeId), // Pass the IndexTypeId values (Chapter, Topic, Subtopic)
            //        Limit = limit // Use the NoOfQuestions limit from tblQuizoo
            //    });
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
        Limit = limit,  // Use the NoOfQuestions limit from tblQuizoo
        CourseID = studentDetails.CourseID
    });

            // Convert the data to a list of DTOs
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
                    // EmployeeName = item.EmpFirstName,
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
                    DurationperQuestion = durationPerQuestion,
                    MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                    AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                };
            });
            var insertQuery = @"
            INSERT INTO tblQuizooQuestions (QuizooID, QuestionID, DisplayOrder)
            VALUES (@QuizooID, @QuestionID, @DisplayOrder)";
            await _connection.ExecuteAsync(insertQuery, questions.Select((q, index) => new
            {
                QuizooID = quizooId,
                QuestionID = q.QuestionId,
                DisplayOrder = index + 1
            }));
            if (response.Count() != 0)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", response.ToList(), 200);
            }
            else
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
            }
        }
        public async Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitAnswerAsync(List<SubmitAnswerRequest> requestList)
        {
            var responseList = new List<AnswerPercentageResponse>();

            foreach (var request in requestList)
            {
                // Query to check if the answer is correct
                string checkCorrectAnswerQuery = @"
        SELECT AMC.IsCorrect
        FROM tblAnswerMaster AM
        INNER JOIN tblAnswerMultipleChoiceCategory AMC ON AM.AnswerID = AMC.AnswerID
        WHERE AM.QuestionID = @QuestionID AND AMC.Answermultiplechoicecategoryid = @Answermultiplechoicecategoryid;";

                // Query to check if the student has already submitted an answer for this question
                string checkExistingAnswerQuery = @"
        SELECT COUNT(1)
        FROM tblQuizooPlayersAnswers
        WHERE QuizooID = @QuizID AND QuestionID = @QuestionID AND StudentID = @StudentID;";

                // Query to update an existing answer
                string updateQuery = @"
        UPDATE tblQuizooPlayersAnswers
        SET AnswerID = @AnswerID, IsCorrect = @IsCorrect
        WHERE QuizooID = @QuizID AND QuestionID = @QuestionID AND StudentID = @StudentID;";

                // Query to insert a new answer
                string insertQuery = @"
        INSERT INTO tblQuizooPlayersAnswers (QuizooID, StudentID, QuestionID, AnswerID, IsCorrect)
        VALUES (@QuizID, @StudentID, @QuestionID, @AnswerID, @IsCorrect);";

                // Check if the answer is correct
                bool isCorrect = await _connection.ExecuteScalarAsync<bool>(checkCorrectAnswerQuery, new
                {
                    QuestionID = request.QuestionID,
                    AnswerID = request.AnswerID,
                    Answermultiplechoicecategoryid = request.AnswerID
                });

                // Check if there is an existing answer for the student
                bool hasExistingAnswer = await _connection.ExecuteScalarAsync<bool>(checkExistingAnswerQuery, new
                {
                    QuizID = request.QuizID,
                    QuestionID = request.QuestionID,
                    StudentID = request.StudentID
                });

                if (hasExistingAnswer)
                {
                    // Update the existing answer
                    await _connection.ExecuteAsync(updateQuery, new
                    {
                        QuizID = request.QuizID,
                        StudentID = request.StudentID,
                        QuestionID = request.QuestionID,
                        AnswerID = request.AnswerID,
                        IsCorrect = isCorrect
                    });
                }
                else
                {
                    // Insert a new answer
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        QuizID = request.QuizID,
                        StudentID = request.StudentID,
                        QuestionID = request.QuestionID,
                        AnswerID = request.AnswerID,
                        IsCorrect = isCorrect
                    });
                }
            }

            // Query to calculate the answer percentages and counts after all submissions
            string percentageQuery = @"
SELECT 
    QPA.QuizooID as QuizID,
    QPA.QuestionID,
    QPA.AnswerID,
    COUNT(QPA.AnswerID) AS AnswerCount,
    (COUNT(QPA.AnswerID) * 100.0 / NULLIF(TotalResponses.TotalCount, 0)) AS AnswerPercentage,
    AMC.Answer AS AnswerText,
    AMC.IsCorrect
FROM 
    tblQuizooPlayersAnswers QPA
INNER JOIN 
    (SELECT COUNT(*) AS TotalCount
     FROM tblQuizooPlayersAnswers
     WHERE QuizooID = @QuizID AND QuestionID = @QuestionID) AS TotalResponses ON 1 = 1
INNER JOIN 
    tblAnswerMultipleChoiceCategory AMC ON QPA.AnswerID = AMC.Answermultiplechoicecategoryid
WHERE 
    QPA.QuizooID = @QuizID 
    AND QPA.QuestionID = @QuestionID
GROUP BY 
    QPA.QuizooID, QPA.QuestionID, QPA.AnswerID, AMC.Answer, AMC.IsCorrect, TotalResponses.TotalCount
ORDER BY 
    AnswerPercentage DESC;";

            // Fetch the updated percentages and counts after processing all submissions
            var percentages = await _connection.QueryAsync<AnswerPercentageResponse>(percentageQuery, new
            {
                QuizID = requestList.First().QuizID,
                QuestionID = requestList.First().QuestionID
            });

            responseList.AddRange(percentages);

            return new ServiceResponse<IEnumerable<AnswerPercentageResponse>>(true, "Answers submitted successfully", responseList.Distinct().ToList(), 200);
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
    }
}