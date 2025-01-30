using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
namespace StudentApp_API.Repository.Implementations
{
    public class ConceptwisePracticeRepository: IConceptwisePracticeRepository
    {
        private readonly IDbConnection _connection;
        public ConceptwisePracticeRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync(@"select * from tblStudentClassCourseMapping where RegistrationID = 
                @RegistrationID", new { RegistrationID = RegistrationId });
                // SQL query to fetch Syllabus Subjects
                string query = @"
            SELECT 
                ss.SyllabusID AS SyllabusId,
                ss.SubjectID AS SubjectId,
                s.SubjectName AS SubjectName 
            FROM tblSyllabusSubjects ss
            INNER JOIN tblSyllabus sy ON ss.SyllabusID = sy.SyllabusId
            INNER JOIN tblSubject s ON ss.SubjectID = s.SubjectID
            WHERE 
                sy.BoardID = @BoardId AND 
                sy.ClassId = @ClassId AND 
                sy.CourseId = @CourseId AND
                sy.Status = 1 
            ";

                var resposne = await _connection.QueryAsync<dynamic>(
                    query, new { BoardId = data.BoardId, ClassId = data.ClassID, CourseId = data.CourseID });
                var subjects = resposne.Select(m => new ConceptwisePracticeSubjectsResposne
                {
                    SubjectId = m.SubjectId,
                    SubjectName = m.SubjectName,
                    SyllabusId = m.SyllabusId,
                    RegistrationId = RegistrationId,
                    Percentage = PercentageCalculation(0, 0, RegistrationId, m.SubjectId, m.SyllabusId)
                }).ToList();
                // Calculate the average percentage
                decimal averagePercentage = subjects.Any()
                    ? subjects.Average(s => s.Percentage)
                    : 0;
                var response = new ConceptwisePracticeResponse
                {
                    conceptwisePracticeSubjectsResposnes = [.. subjects],
                    Percentage = Math.Round(averagePercentage, 2)
                };
                // Check if any subjects were found
                if (subjects != null && subjects.Count != 0)
                {
                    return new ServiceResponse<ConceptwisePracticeResponse>(true, "Subjects retrieved successfully.", response, 200);
                }
                else
                {
                    return new ServiceResponse<ConceptwisePracticeResponse>(false, "No subjects found for the given criteria.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ConceptwisePracticeResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            List<ConceptwisePracticeContentResponse> contentResponse = new List<ConceptwisePracticeContentResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Fetch chapters (IndexTypeId = 1) when SyllabusId and SubjectId are provided
                if (request.SyllabusId.HasValue && request.SubjectId.HasValue && request.IndexTypeId == 0 && request.ContentIndexId == 0)
                {
                    string queryChapters = @"
                SELECT c.ContentIndexId AS ContentId, c.SubjectId, c.ContentName_Chapter AS ContentName, s.SyllabusID, c.IndexTypeId
                FROM tblContentIndexChapters c
                LEFT JOIN tblSyllabusDetails s ON c.ContentIndexId = s.ContentIndexId
                WHERE s.SyllabusID = @SyllabusId AND c.SubjectId = @SubjectId AND c.IndexTypeId = 1 AND c.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(queryChapters, new
                    {
                        SyllabusId = request.SyllabusId,
                        SubjectId = request.SubjectId
                    })).ToList();
                }
                // Fetch topics (children of chapters) if IndexTypeId = 1 and ContentId (chapter) is provided
                else if (request.IndexTypeId == 1 && request.ContentIndexId.HasValue)
                {
                    string queryTopics = @"
                SELECT t.ContInIdTopic AS ContentId, t.ContentName_Topic AS ContentName, s.SyllabusID, t.IndexTypeId
                FROM tblContentIndexTopics t
                LEFT JOIN tblSyllabusDetails s ON t.ContentIndexId = s.ContentIndexId
                WHERE t.ContentIndexId = @ContentIndexId AND t.IndexTypeId = 2 AND t.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(queryTopics, new
                    {
                        ContentIndexId = request.ContentIndexId
                    })).ToList();
                }
                // Fetch subtopics (children of topics) if IndexTypeId = 2 and ContentId (topic) is provided
                else if (request.IndexTypeId == 2 && request.ContentIndexId.HasValue)
                {
                    string querySubTopics = @"
                SELECT s.ContInIdSubTopic AS ContentId, s.ContentName_SubTopic AS ContentName, d.SyllabusID, s.IndexTypeId
                FROM tblContentIndexSubTopics s
                LEFT JOIN tblSyllabusDetails d ON s.ContInIdTopic = d.ContentIndexId
                WHERE s.ContInIdTopic = @ContentIndexId AND s.IndexTypeId = 3 AND s.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(querySubTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                    })).ToList();
                }
                foreach (var data in contentResponse)
                {
                    data.RegistrationId = request.RegistrationId;
                    data.Percentage = PercentageCalculation(data.IndexTypeId, data.ContentId, request.RegistrationId, request.SubjectId, request.SyllabusId);
                }
                return new ServiceResponse<List<ConceptwisePracticeContentResponse>>(true, "Success", contentResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ConceptwisePracticeContentResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsAsync(GetQuestionsList request)
        {
            // Query to get the difficulty levels and the number of questions per level
            string difficultyLevelQuery = @"
    SELECT 
        LevelId, 
        LevelName, 
        NoofQperLevel 
    FROM tbldifficultylevel 
    WHERE Status = 1";

            // Query to fetch questions based on difficulty levels
            string questionQuery = @"
    SELECT 
        q.QuestionId,
        q.QuestionCode,
        q.QuestionDescription,
        q.QuestionFormula,
        q.QuestionImage,
        qc.LevelId,
        q.QuestionTypeId,
        q.IndexTypeId,
        q.Status,
        q.Explanation,
        q.IsActive,
        q.IsLive,
        q.ExtraInformation,
        q.IsConfigure,
        q.CategoryId,
        qt.QuestionType AS QuestionType
    FROM tblQuestion q
    INNER JOIN tblQIDCourse qc ON q.QuestionId = qc.QID
    LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
    WHERE q.SubjectId = @SubjectId
    AND q.IndexTypeId = @IndexTypeId
    AND q.ContentIndexId = @ContentIndexId
    AND qc.LevelId = @DifficultyLevelId AND qc.CourseID = @CourseID
    AND q.IsActive = 1 
    AND q.IsLive = 1
    ORDER BY q.QuestionId";


            // Fetch difficulty levels
            var difficultyLevels = await _connection.QueryAsync<DifficultyLevelDTO>(difficultyLevelQuery);

            // Initialize the result list
            var result = new List<QuestionResponseDTO>();

            // Fetch questions for each difficulty level
            foreach (var level in difficultyLevels)
            {
                var questions = await _connection.QueryAsync<QuestionResponseDTO>(questionQuery, new
                {
                    SubjectId = request.subjectId,
                    IndexTypeId = request.indexTypeId,
                    ContentIndexId = request.contentId,
                    DifficultyLevelId = level.LevelId,
                    CourseID = request.CourseId
                });

                // Take only the number of questions specified for this difficulty level
                result.AddRange(questions.Take(level.NoofQperLevel));
            }
            var response = result.Select(item =>
            {
                if (item.QuestionTypeId == 11)
                {
                    return new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        Paragraph = item.Paragraph,
                        SubjectName = item.SubjectName,
                      //  EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                      //  QIDCourses = GetListOfQIDCourse(item.QuestionCode),
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
                }
                else
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
                        MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                        MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                        Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                        AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                    };
                }
            }).ToList();
            // Filter the questions that are not already present in the table

            // Query to check if a question exists in tblConceptwisePracticeQuestions
            string checkQuestionExistsQuery = @"
    SELECT 1 
    FROM tblConceptwisePracticeQuestions
    WHERE QuestionID = @QuestionID
    AND IndexTypeID = @IndexTypeID
    AND ContentID = @ContentID
    AND SyllabusID = @SyllabusID";

            // Query to insert new records into tblConceptwisePracticeQuestions
            string insertQuestionQuery = @"
    INSERT INTO tblConceptwisePracticeQuestions (ContentID, IndexTypeID, QuestionID, SyllabusID)
    VALUES (@ContentID, @IndexTypeID, @QuestionID, @SyllabusID)";
            foreach (var question in response)
            {
                bool exists = await _connection.ExecuteScalarAsync<bool>(checkQuestionExistsQuery, new
                {
                    QuestionID = question.QuestionId,
                    IndexTypeID = request.indexTypeId,
                    ContentID = request.contentId,
                    SyllabusID = request.SyllabusId
                });

                if (!exists)
                {
                    // Insert the new question into tblConceptwisePracticeQuestions
                    await _connection.ExecuteAsync(insertQuestionQuery, new
                    {
                        ContentID = request.contentId,
                        IndexTypeID = request.indexTypeId,
                        QuestionID = question.QuestionId,
                        SyllabusID = request.SyllabusId,
                       // SetID = request.setId
                    });

                   // result.Add(question);
                }
            }
            return new ServiceResponse<List<QuestionResponseDTO>>(true, "Records found successfully", response, 200, response.Count);
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);
            try
            {
                // Check if the question is already saved by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT CPQSId 
              FROM [tblConceptwisePracticeQuestionSave] 
              WHERE StudentId = @StudentId AND QuestionId = @QuestionId",
                      new { StudentId = request.RegistrationId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it
                    var deleteQuery = @"DELETE FROM [tblConceptwisePracticeQuestionSave] 
                                WHERE StudentId = @RegistrationId AND QuestionId = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId
                    });

                    if (rowsDeleted > 0)
                    {
                        response.Data = "Question unsaved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to unsave the question.";
                        response.Success = false;
                    }
                }
                else
                {
                    // If no record exists, insert a new saved question record
                    var insertQuery = @"INSERT INTO [tblConceptwisePracticeQuestionSave] (StudentId, QuestionId, QuestionCode) 
                                VALUES (@RegistrationId, @QuestionId, @QuestionCode)";

                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    if (rowsInserted > 0)
                    {
                        response.Data = "Question saved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to save the question.";
                        response.Success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                response.Data = $"An error occurred: {ex.Message}";
                response.Success = false;
            }

            return response;
        }
        public async Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            var response = new ConceptwiseAnswerResponse
            {
                QuestionID = request.QuestionID
            };

            try
            {
                // Fetch QuestionTypeId for the given QuestionID
                const string questionTypeQuery = @"
        SELECT QuestionTypeid 
        FROM tblAnswerMaster
        WHERE Questionid = @QuestionID";

                var questionTypeId = await _connection.ExecuteScalarAsync<int>(questionTypeQuery, new { request.QuestionID });

                bool isCorrect = false;
                bool isPartiallyCorrect = false;

                switch (questionTypeId)
                {
                    case 1:
                    case 2:
                    case 5:
                    case 10:
                    case 13:
                    case 15: // Multi-choice types
                        response = await HandleMultipleChoiceAnswerAsync(request);
                        isCorrect = response.IsAnswerCorrect;
                        // Check if it's a multiple answer question (MAQ) and partially correct
                        if (questionTypeId == 5 && !isCorrect)
                        {
                            isPartiallyCorrect = await IsPartiallyCorrectAnswerAsync(request);
                        }
                        break;

                    case 3:
                    case 4:
                    case 7:
                    case 8:
                    case 9: // Single-answer types
                        response = await HandleSingleAnswerAsync(request);
                        isCorrect = response.IsAnswerCorrect;
                        break;

                    case 6: // Match the Pair
                        response = await HandleMatchThePairAsync(request);
                        isCorrect = response.IsAnswerCorrect;
                        break;

                    case 12: // Match the Pair 2
                        response = await HandleMatchThePair2Async(request);
                        isCorrect = response.IsAnswerCorrect;
                        break;

                    default:
                        throw new Exception("Unsupported question type.");
                }
                // Determine the AnswerStatusId based on the correctness
                int answerStatusId = 2; // Default to Incorrect Answered

                if (isCorrect)
                    answerStatusId = 1; // Correct Answered
                else if (isPartiallyCorrect)
                    answerStatusId = 4; // Partially Correct Answered
                else if (string.IsNullOrEmpty(request.AnswerID))
                    answerStatusId = 3; // UnAttempted Questions

                // Calculate time taken in seconds with decimal precision
                decimal? timeTaken = (request.StaTime.HasValue && request.EndTime.HasValue)
                    ? (decimal?)(request.EndTime.Value - request.StaTime.Value).TotalSeconds
                    : null;

                // Insert answer submission into tblConceptwisePracticeAnswers
                const string insertAnswerQuery = @"
        INSERT INTO tblConceptwisePracticeAnswers (StudentId, QuestionId, AnswerIds, StaTime, EndTime, TimeTaken, IsCorrect, AnswerStatusId)
        VALUES (@StudentId, @QuestionId, @AnswerIds, @StaTime, @EndTime, @TimeTaken, @IsCorrect, @AnswerStatusId)";

                await _connection.ExecuteAsync(insertAnswerQuery, new
                {
                    StudentId = request.StudentID,
                    QuestionId = request.QuestionID,
                    AnswerIds = request.AnswerID,
                    StaTime = request.StaTime,
                    EndTime = request.EndTime,
                    TimeTaken = timeTaken,  // Now stores decimal values like 5.23
                    IsCorrect = isCorrect,
                    AnswerStatusId = answerStatusId
                });

                return new ServiceResponse<ConceptwiseAnswerResponse>(true, "Answer processed successfully", response, 200);

            }
            catch (Exception ex)
            {
                return new ServiceResponse<ConceptwiseAnswerResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<decimal>> GetStudentQuestionAccuracyAsync(int studentId, int questionId)
        {
            try
            {
                const string query = @"
        WITH Attempts AS (
            SELECT 
                QuestionId, 
                StudentId, 
                COUNT(*) AS TotalAttempts,  -- Count all attempts (correct & incorrect)
                MIN(CPAId) AS FirstCorrectAttempt  -- First correct attempt
            FROM tblConceptwisePracticeAnswers
            WHERE StudentId = @StudentId AND QuestionId = @QuestionId  
            GROUP BY QuestionId, StudentId
        )
        SELECT 
            CAST(100.0 / TotalAttempts AS DECIMAL(10,2)) AS AccuracyRate
        FROM Attempts;";

                var accuracyRate = await _connection.ExecuteScalarAsync<decimal?>(query, new { StudentId = studentId, QuestionId = questionId });

                return new ServiceResponse<decimal>(true, "Accuracy calculated successfully", accuracyRate ?? 0, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<decimal>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<decimal>> GetStudentGroupAccuracyForQuestionAsync(int studentId, int questionId)
        {
            try
            {
                string query = @"WITH StudentGroup AS (
    SELECT BoardId, ClassID, CourseID
    FROM tblStudentClassCourseMapping
    WHERE RegistrationID = @StudentID
),
MatchedStudents AS (
    SELECT RegistrationID AS StudentID
    FROM tblStudentClassCourseMapping
    WHERE BoardId = (SELECT BoardId FROM StudentGroup)
    AND ClassID = (SELECT ClassID FROM StudentGroup)
    AND CourseID = (SELECT CourseID FROM StudentGroup)
    AND RegistrationID <> @StudentID  -- Exclude the given student
),
Attempts AS (
    SELECT 
        c.StudentId,
        c.QuestionId,
        SUM(CASE WHEN c.IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectAttempts,
        COUNT(*) AS TotalAttempts
    FROM tblConceptwisePracticeAnswers c
    JOIN MatchedStudents s ON c.StudentId = s.StudentID
    WHERE c.QuestionId = @QuestionId  -- Filter for the given question
    GROUP BY c.StudentId, c.QuestionId
),
StudentAccuracy AS (
    SELECT 
        StudentID,
        CAST((CorrectAttempts * 100.0) / TotalAttempts AS DECIMAL(10,2)) AS AccuracyRate
    FROM Attempts
)
SELECT 
    COALESCE(AVG(AccuracyRate), 0) AS AverageAccuracy
FROM StudentAccuracy;
";

                decimal accuracy = await _connection.ExecuteScalarAsync<decimal>(query, new { StudentID = studentId, QuestionID = questionId });

                return new ServiceResponse<decimal>(true, "Average accuracy calculated successfully", accuracy, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<decimal>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<double>> GetAverageTimeSpentByOtherStudents(int studentId, int questionId)
        {
            // Step 1: Fetch the BoardId, ClassID, and CourseID for the given student
            string studentQuery = @"
        SELECT BoardId, ClassID, CourseID
        FROM tblStudentClassCourseMapping
        WHERE RegistrationID = @StudentID";

            var studentMapping = await _connection.QuerySingleOrDefaultAsync(studentQuery, new { StudentID = studentId });

            if (studentMapping == null)
            {
                return new ServiceResponse<double>(false, "Student not found.", 0, 500);
            }

            // Step 2: Fetch other students in the same BoardId, ClassID, CourseID
            string classCourseQuery = @"
        SELECT RegistrationID
        FROM tblStudentClassCourseMapping
        WHERE BoardId = @BoardId AND ClassID = @ClassID AND CourseID = @CourseID AND RegistrationID != @StudentID";

            var otherStudents = await _connection.QueryAsync<int>(classCourseQuery, new
            {
                BoardId = studentMapping.BoardId,
                ClassID = studentMapping.ClassID,
                CourseID = studentMapping.CourseID,
                StudentID = studentId
            });

            if (!otherStudents.Any())
            {
                return new ServiceResponse<double>(true, "No other students found for the same board, class, and course.", 0, 200);
            }

            // Step 3: Fetch TimeTaken for other students on the same question
            string timeQuery = @"
        SELECT TimeTaken 
        FROM tblConceptwisePracticeAnswers cpa
        WHERE cpa.StudentId IN @StudentIds AND cpa.QuestionId = @QuestionId";

            var records = await _connection.QueryAsync<double?>(timeQuery, new
            {
                StudentIds = otherStudents,
                QuestionId = questionId
            });

            // Step 4: Calculate the average time
            double totalTimeTaken = 0;
            int count = 0;

            foreach (var record in records)
            {
                if (record.HasValue)
                {
                    totalTimeTaken += record.Value;
                    count++;
                }
            }

            if (count == 0)
            {
                return new ServiceResponse<double>(true, "No time records found for other students on this question.", 0, 200);
            }

            // Calculate average time
            double averageTime = totalTimeTaken / count;

            // Return the service response with the calculated average time
            return new ServiceResponse<double>(true, "Average time calculated successfully for other students.", averageTime, 200);
        }
        public async Task<ServiceResponse<QuestionAttemptStatsResponse>> GetQuestionAttemptStatsForGroupAsync(int studentId, int questionId)
        {
            try
            {
                string query = @"WITH StudentGroup AS (
    SELECT BoardId, ClassID, CourseID
    FROM tblStudentClassCourseMapping
    WHERE RegistrationID = @StudentID
),
MatchedStudents AS (
    SELECT RegistrationID AS StudentID
    FROM tblStudentClassCourseMapping
    WHERE BoardId = (SELECT BoardId FROM StudentGroup)
      AND ClassID = (SELECT ClassID FROM StudentGroup)
      AND CourseID = (SELECT CourseID FROM StudentGroup)
      AND RegistrationID <> @StudentID  -- Exclude the given student
)
SELECT 
    COUNT(DISTINCT c.StudentId) AS TotalAttempts,
    COUNT(DISTINCT CASE WHEN c.IsCorrect = 1 THEN c.StudentId END) AS CorrectAnswers
FROM tblConceptwisePracticeAnswers c
JOIN MatchedStudents s ON c.StudentId = s.StudentID
WHERE c.QuestionId = @QuestionID;
";
                var result = await _connection.QueryFirstOrDefaultAsync<QuestionAttemptStatsResponse>(
                    query, new { StudentID = studentId, QuestionID = questionId });

                if (result == null)
                    result = new QuestionAttemptStatsResponse { TotalAttempts = 0, CorrectAnswers = 0 };

                return new ServiceResponse<QuestionAttemptStatsResponse>(
                    true, "Question attempt stats retrieved successfully", result, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionAttemptStatsResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<double>> GetAverageTimeSpentOnQuestion(int studentId, int questionId)
        {
            string query = @"
    SELECT StaTime, EndTime, TimeTaken 
    FROM tblConceptwisePracticeAnswers
    WHERE StudentId = @StudentId AND QuestionId = @QuestionId";

            try
            {
                var records = await _connection.QueryAsync(query, new { StudentId = studentId, QuestionId = questionId });

                double totalTimeTaken = 0;
                int count = 0;

                foreach (var record in records)
                {
                    // Check if TimeTaken is not NULL
                    if (record.TimeTaken != null)
                    {
                        totalTimeTaken += Convert.ToDouble(record.TimeTaken); // Convert to double
                    }
                    else if (record.StaTime != null && record.EndTime != null)
                    {
                        // If TimeTaken is NULL, calculate from StaTime and EndTime
                        totalTimeTaken += ((DateTime)record.EndTime - (DateTime)record.StaTime).TotalSeconds;
                    }
                    count++;
                }

                if (count == 0)
                {
                    return new ServiceResponse<double>(false, "No records found", 0, 500);
                }

                double averageTime = totalTimeTaken / count;

                return new ServiceResponse<double>(true, "Records found", averageTime, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<double>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<PracticeStatsDto>> GetPracticeQuestionStats(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<PracticeStatsDto>(true, string.Empty, null, 200);
            try
            {
               
                    string sqlQuery = @"
            WITH LatestAttempts AS (
                SELECT 
                    cpa.QuestionId,
                    cpa.StudentId,
                    cpa.AnswerStatusId,
                    ROW_NUMBER() OVER (PARTITION BY cpa.StudentId, cpa.QuestionId ORDER BY cpa.CPAId DESC) AS rn
                FROM tblConceptwisePracticeAnswers cpa
            ),
            FilteredAnswers AS (
                SELECT 
                    la.QuestionId,
                    la.StudentId,
                    la.AnswerStatusId
                FROM LatestAttempts la
                WHERE la.rn = 1
            )
            SELECT 
                COUNT(CASE WHEN fa.AnswerStatusId = 1 THEN 1 END) AS CorrectAnswers,
                COUNT(CASE WHEN fa.AnswerStatusId = 2 THEN 1 END) AS IncorrectAnswers,
                COUNT(CASE WHEN fa.AnswerStatusId IS NULL THEN 1 END) AS UnattemptedQuestions,
                COUNT(CASE WHEN fa.AnswerStatusId = 4 THEN 1 END) AS PartiallyCorrect,
                COUNT(cpq.QuestionId) AS TotalQuestions,
                COUNT(fa.QuestionId) AS TotalAnswered,
                (COUNT(CASE WHEN fa.AnswerStatusId = 1 THEN 1 END) * 100.0) / NULLIF(COUNT(cpq.QuestionId), 0) AS CorrectPercentage,
                (COUNT(CASE WHEN fa.AnswerStatusId = 2 THEN 1 END) * 100.0) / NULLIF(COUNT(cpq.QuestionId), 0) AS IncorrectPercentage,
                (COUNT(CASE WHEN fa.AnswerStatusId IS NULL THEN 1 END) * 100.0) / NULLIF(COUNT(cpq.QuestionId), 0) AS UnattemptedPercentage,
                (COUNT(CASE WHEN fa.AnswerStatusId = 4 THEN 1 END) * 100.0) / NULLIF(COUNT(cpq.QuestionId), 0) AS PartiallyCorrectPercentage
            FROM tblConceptwisePracticeQuestions cpq
            LEFT JOIN FilteredAnswers fa 
                ON cpq.QuestionId = fa.QuestionId 
                AND fa.StudentId = @StudentId
            WHERE cpq.IndexTypeID = @IndexTypeID
            AND cpq.ContentID = @ContentID
            AND cpq.SyllabusID = @SyllabusID;";

                    var parameters = new
                    {
                        StudentId = studentId,
                        IndexTypeID = indexTypeId,
                        ContentID = contentId,
                        SyllabusID = syllabusId
                    };

                    var result = await _connection.QueryFirstOrDefaultAsync<PracticeStatsDto>(sqlQuery, parameters);

                    if (result != null)
                    {
                        response.Success = true;
                        response.Message = "Data retrieved successfully";
                        response.Data = result;
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "No data found";
                    }
                
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        //public async Task<ServiceResponse<AccuracyRateDto>> GetStudentAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId)
        //{
        //    var response = new ServiceResponse<AccuracyRateDto>(true, string.Empty, null, 200);

        //    try
        //    {

        //            string sqlQuery = @"
        //    WITH QuestionAttempts AS (
        //        SELECT 
        //            cpa.QuestionId,
        //            cpa.StudentId,
        //            cpa.AnswerStatusId
        //        FROM tblConceptwisePracticeAnswers cpa
        //        INNER JOIN tblConceptwisePracticeQuestions cpq
        //            ON cpa.QuestionId = cpq.QuestionId
        //        WHERE cpq.IndexTypeID = @IndexTypeID
        //        AND cpq.ContentID = @ContentID
        //        AND cpq.SyllabusID = @SyllabusID
        //        AND cpa.StudentId = @StudentId
        //    )
        //    SELECT 
        //        COUNT(DISTINCT cpq.QuestionId) AS TotalQuestions,
        //        COUNT(qa.QuestionId) AS TotalAttempts,
        //        COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) AS TotalCorrectAttempts,
        //        (COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) * 100.0) / NULLIF(COUNT(qa.QuestionId), 0) AS AccuracyRate
        //    FROM tblConceptwisePracticeQuestions cpq
        //    LEFT JOIN QuestionAttempts qa 
        //        ON cpq.QuestionId = qa.QuestionId
        //    WHERE cpq.IndexTypeID = @IndexTypeID
        //    AND cpq.ContentID = @ContentID
        //    AND cpq.SyllabusID = @SyllabusID;";

        //            var parameters = new
        //            {
        //                StudentId = studentId,
        //                IndexTypeID = indexTypeId,
        //                ContentID = contentId,
        //                SyllabusID = syllabusId
        //            };

        //            var result = await _connection.QueryFirstOrDefaultAsync<AccuracyRateDto>(sqlQuery, parameters);

        //            if (result != null)
        //            {
        //                response.Success = true;
        //                response.Message = "Accuracy rate calculated successfully";
        //                response.Data = result;
        //            }
        //            else
        //            {
        //                response.Success = false;
        //                response.Message = "No data found";
        //            }

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Success = false;
        //        response.Message = $"Error: {ex.Message}";
        //    }

        //    return response;
        //}
        public async Task<ServiceResponse<AccuracyRateDto>> GetStudentAndClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<AccuracyRateDto>(true, string.Empty, null, 200);

            try
            {
                using var connection = _connection;

                // Fetch student's class, course, and board details
                string studentClassQuery = @"
        SELECT ClassID, CourseID, BoardID 
        FROM tblStudentClassCourseMapping 
        WHERE RegistrationID = @StudentId;";

                var studentClass = await connection.QueryFirstOrDefaultAsync(studentClassQuery, new { StudentId = studentId });

                if (studentClass == null)
                {
                    response.Success = false;
                    response.Message = "Student details not found.";
                    return response;
                }

                // Fetch classmates (excluding the given student)
                string classmatesQuery = @"
        SELECT RegistrationID 
        FROM tblStudentClassCourseMapping 
        WHERE ClassID = @ClassID 
          AND CourseID = @CourseID 
          AND BoardID = @BoardID 
          AND RegistrationID <> @StudentId;";

                var classmates = (await connection.QueryAsync<int>(classmatesQuery, new
                {
                    ClassID = studentClass.ClassID,
                    CourseID = studentClass.CourseID,
                    BoardID = studentClass.BoardID,
                    StudentId = studentId
                })).ToList();

                // Query to calculate accuracy rate for the given student
                string accuracyQuery = @"
        WITH QuestionAttempts AS (
            SELECT 
                cpa.QuestionId, 
                cpa.StudentId, 
                cpa.AnswerStatusId
            FROM tblConceptwisePracticeAnswers cpa
            INNER JOIN tblConceptwisePracticeQuestions cpq
                ON cpa.QuestionId = cpq.QuestionId
            WHERE cpq.IndexTypeID = @IndexTypeID
              AND cpq.ContentID = @ContentID
              AND cpq.SyllabusID = @SyllabusID
              AND cpa.StudentId = @StudentId
        )
        SELECT 
            @StudentId AS StudentId,
            COUNT(DISTINCT cpq.QuestionId) AS TotalQuestions,
            COUNT(qa.QuestionId) AS TotalAttempts,
            COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) AS TotalCorrectAttempts,
            (COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) * 100.0) / NULLIF(COUNT(qa.QuestionId), 0) AS AccuracyRate
        FROM tblConceptwisePracticeQuestions cpq
        LEFT JOIN QuestionAttempts qa 
            ON cpq.QuestionId = qa.QuestionId
        WHERE cpq.IndexTypeID = @IndexTypeID
          AND cpq.ContentID = @ContentID
          AND cpq.SyllabusID = @SyllabusID;";

                var studentAccuracy = await connection.QueryFirstOrDefaultAsync<AccuracyRateDto>(accuracyQuery, new
                {
                    StudentId = studentId,
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                });

                if (studentAccuracy == null)
                {
                    response.Success = false;
                    response.Message = "No accuracy data found for the student.";
                    return response;
                }

                // Fetch accuracy data for classmates
                List<AccuracyRateDto> classmatesAccuracy = new List<AccuracyRateDto>();
                int higherAccuracyCount = 0;

                foreach (var classmateId in classmates)
                {
                    var classmateAccuracy = await connection.QueryFirstOrDefaultAsync<AccuracyRateDto>(accuracyQuery, new
                    {
                        StudentId = classmateId,
                        IndexTypeID = indexTypeId,
                        ContentID = contentId,
                        SyllabusID = syllabusId
                    });

                    if (classmateAccuracy != null)
                    {
                        classmatesAccuracy.Add(classmateAccuracy);

                        // Count classmates with higher accuracy
                        if (classmateAccuracy.AccuracyRate > studentAccuracy.AccuracyRate)
                        {
                            higherAccuracyCount++;
                        }
                    }
                }

                // Calculate the average accuracy rate of classmates
                decimal othersAccuracyRate = classmatesAccuracy.Any()
                    ? classmatesAccuracy.Average(x => x.AccuracyRate)
                    : 0;

                // Set response data
                studentAccuracy.OthersAccuracyRate = othersAccuracyRate;
                studentAccuracy.HigherAccuracyCount = higherAccuracyCount;
                // Query to count distinct students who attempted questions
                string attemptedStudentsQuery = @"
SELECT COUNT(DISTINCT cpa.StudentId) 
FROM tblConceptwisePracticeAnswers cpa
INNER JOIN tblConceptwisePracticeQuestions cpq 
    ON cpa.QuestionId = cpq.QuestionId
WHERE cpq.IndexTypeID = @IndexTypeID
  AND cpq.ContentID = @ContentID
  AND cpq.SyllabusID = @SyllabusID;";

                int attemptedStudentsCount = await connection.ExecuteScalarAsync<int>(attemptedStudentsQuery, new
                {
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                });

                // Set the count in response
                studentAccuracy.AttemptedStudentsCount = attemptedStudentsCount;


                response.Success = true;
                response.Message = "Accuracy rate calculated successfully.";
                response.Data = studentAccuracy;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        //public async Task<ServiceResponse<TimeSpentDto>> GetTotalAndAverageTimeSpent(int studentId, int indexTypeId, int contentId, int syllabusId)
        //{
        //    var response = new ServiceResponse<TimeSpentDto>(true, string.Empty, null, 200);

        //    try
        //    {
        //            string sqlQuery = @"
        //    WITH QuestionAttempts AS (
        //        SELECT 
        //            cpa.StudentId,
        //            cpa.QuestionId,
        //            cpa.TimeTaken
        //        FROM tblConceptwisePracticeAnswers cpa
        //        INNER JOIN tblConceptwisePracticeQuestions cpq
        //            ON cpa.QuestionId = cpq.QuestionId
        //        WHERE cpq.IndexTypeID = @IndexTypeID
        //        AND cpq.ContentID = @ContentID
        //        AND cpq.SyllabusID = @SyllabusID
        //        AND cpa.StudentId = @StudentId
        //    )
        //    SELECT 
        //        COUNT(DISTINCT qa.QuestionId) AS TotalQuestions,
        //        COUNT(qa.QuestionId) AS TotalAttempts,
        //        SUM(qa.TimeTaken) AS TotalTimeSpent,
        //        CASE 
        //            WHEN COUNT(DISTINCT qa.QuestionId) > 0 
        //            THEN SUM(qa.TimeTaken) / COUNT(DISTINCT qa.QuestionId) 
        //            ELSE 0 
        //        END AS AverageTimePerQuestion
        //    FROM QuestionAttempts qa;";

        //            var parameters = new
        //            {
        //                StudentId = studentId,
        //                IndexTypeID = indexTypeId,
        //                ContentID = contentId,
        //                SyllabusID = syllabusId
        //            };

        //            var result = await _connection.QueryFirstOrDefaultAsync<TimeSpentDto>(sqlQuery, parameters);

        //            if (result != null)
        //            {
        //                response.Success = true;
        //                response.Message = "Total and average time spent calculated successfully";
        //                response.Data = result;
        //            }
        //            else
        //            {
        //                response.Success = false;
        //                response.Message = "No data found";
        //            }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Success = false;
        //        response.Message = $"Error: {ex.Message}";
        //    }

        //    return response;
        //}
        public async Task<ServiceResponse<TimeSpentDto>> GetTotalAndAverageTimeSpent(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<TimeSpentDto>(true, string.Empty, null, 200);

            try
            {
                using var connection = _connection;

                // Fetch student's class, course, and board details
                string studentClassQuery = @"
        SELECT ClassID, CourseID, BoardID 
        FROM tblStudentClassCourseMapping 
        WHERE RegistrationID = @StudentId;";

                var studentClass = await connection.QueryFirstOrDefaultAsync(studentClassQuery, new { StudentId = studentId });

                if (studentClass == null)
                {
                    response.Success = false;
                    response.Message = "Student details not found.";
                    return response;
                }

                // Fetch classmates (excluding the given student)
                string classmatesQuery = @"
        SELECT RegistrationID 
        FROM tblStudentClassCourseMapping 
        WHERE ClassID = @ClassID 
          AND CourseID = @CourseID 
          AND BoardID = @BoardID 
          AND RegistrationID <> @StudentId;";

                var classmates = (await connection.QueryAsync<int>(classmatesQuery, new
                {
                    ClassID = studentClass.ClassID,
                    CourseID = studentClass.CourseID,
                    BoardID = studentClass.BoardID,
                    StudentId = studentId
                })).ToList();

                // Query to calculate student's time spent
                string studentTimeQuery = @"
        WITH QuestionAttempts AS (
            SELECT 
                cpa.StudentId,
                cpa.QuestionId,
                cpa.TimeTaken
            FROM tblConceptwisePracticeAnswers cpa
            INNER JOIN tblConceptwisePracticeQuestions cpq
                ON cpa.QuestionId = cpq.QuestionId
            WHERE cpq.IndexTypeID = @IndexTypeID
            AND cpq.ContentID = @ContentID
            AND cpq.SyllabusID = @SyllabusID
            AND cpa.StudentId = @StudentId
        )
        SELECT 
            COUNT(DISTINCT qa.QuestionId) AS TotalQuestions,
            COUNT(qa.QuestionId) AS TotalAttempts,
            SUM(qa.TimeTaken) AS TotalTimeSpent,
            CASE 
                WHEN COUNT(DISTINCT qa.QuestionId) > 0 
                THEN SUM(qa.TimeTaken) / COUNT(DISTINCT qa.QuestionId) 
                ELSE 0 
            END AS AverageTimePerQuestion
        FROM QuestionAttempts qa;";

                var studentResult = await connection.QueryFirstOrDefaultAsync<TimeSpentDto>(studentTimeQuery, new
                {
                    StudentId = studentId,
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                });

                if (studentResult == null)
                {
                    response.Success = false;
                    response.Message = "No data found for the student.";
                    return response;
                }

                // Query to calculate classmates' time spent
                string classmatesTimeQuery = @"
        WITH QuestionAttempts AS (
            SELECT 
                cpa.StudentId,
                cpa.QuestionId,
                cpa.TimeTaken
            FROM tblConceptwisePracticeAnswers cpa
            INNER JOIN tblConceptwisePracticeQuestions cpq
                ON cpa.QuestionId = cpq.QuestionId
            WHERE cpq.IndexTypeID = @IndexTypeID
            AND cpq.ContentID = @ContentID
            AND cpq.SyllabusID = @SyllabusID
            AND cpa.StudentId IN @Classmates
        )
        SELECT 
            COUNT(DISTINCT qa.QuestionId) AS ClassmatesTotalQuestions,
            COUNT(qa.QuestionId) AS ClassmatesTotalAttempts,
            SUM(qa.TimeTaken) AS ClassmatesTotalTimeSpent,
            CASE 
                WHEN COUNT(DISTINCT qa.QuestionId) > 0 
                THEN SUM(qa.TimeTaken) / COUNT(DISTINCT qa.QuestionId) 
                ELSE 0 
            END AS ClassmatesAverageTimePerQuestion
        FROM QuestionAttempts qa;";

                var classmatesResult = await connection.QueryFirstOrDefaultAsync(classmatesTimeQuery, new
                {
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId,
                    Classmates = classmates
                });

                // Assign classmates' data
                studentResult.ClassmatesTotalTimeSpent = classmatesResult?.ClassmatesTotalTimeSpent ?? 0;
                studentResult.ClassmatesAverageTimePerQuestion = classmatesResult?.ClassmatesAverageTimePerQuestion ?? 0;

                response.Success = true;
                response.Message = "Total and average time spent calculated successfully.";
                response.Data = studentResult;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        public async Task<ServiceResponse<AnswerTimeStatsDto>> GetAnswerTimeStats(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<AnswerTimeStatsDto>(true, string.Empty, null, 200);

            try
            {

                string sqlQuery = @"
            WITH QuestionData AS (
                SELECT 
                    cpq.QuestionId
                FROM tblConceptwisePracticeQuestions cpq
                WHERE cpq.IndexTypeID = @IndexTypeID
                AND cpq.ContentID = @ContentID
                AND cpq.SyllabusID = @SyllabusID
            ),
            AnswerData AS (
                SELECT 
                    cpa.QuestionId,
                    cpa.TimeTaken,
                    cpa.IsCorrect,
                    cpa.AnswerStatusId
                FROM tblConceptwisePracticeAnswers cpa
                INNER JOIN QuestionData qd 
                    ON cpa.QuestionId = qd.QuestionId
                WHERE cpa.StudentId = @StudentId
            )
            SELECT 
                -- Total Time
                COALESCE(SUM(CASE WHEN a.IsCorrect = 1 THEN a.TimeTaken ELSE 0 END), 0) AS TotalTimeCorrect,
                COALESCE(SUM(CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 2 THEN a.TimeTaken ELSE 0 END), 0) AS TotalTimeIncorrect,
                COALESCE(SUM(CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 3 THEN a.TimeTaken ELSE 0 END), 0) AS TotalTimePartiallyCorrect,

                -- Average Time per Question
                COALESCE(NULLIF(SUM(CASE WHEN a.IsCorrect = 1 THEN a.TimeTaken ELSE 0 END), 0) / NULLIF(COUNT(DISTINCT CASE WHEN a.IsCorrect = 1 THEN a.QuestionId END), 0), 0) AS AverageTimeCorrect,
                COALESCE(NULLIF(SUM(CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 2 THEN a.TimeTaken ELSE 0 END), 0) / NULLIF(COUNT(DISTINCT CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 2 THEN a.QuestionId END), 0), 0) AS AverageTimeIncorrect,
                COALESCE(NULLIF(SUM(CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 3 THEN a.TimeTaken ELSE 0 END), 0) / NULLIF(COUNT(DISTINCT CASE WHEN a.IsCorrect = 0 AND a.AnswerStatusId = 3 THEN a.QuestionId END), 0), 0) AS AverageTimePartiallyCorrect,

                -- Total Unattempted Questions
                COALESCE(COUNT(DISTINCT q.QuestionId) - COUNT(DISTINCT a.QuestionId), 0) AS TotalUnattempted
            FROM QuestionData q
            LEFT JOIN AnswerData a 
                ON q.QuestionId = a.QuestionId;";

                var parameters = new
                {
                    StudentId = studentId,
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                };

                var result = await _connection.QueryFirstOrDefaultAsync<AnswerTimeStatsDto>(sqlQuery, parameters);

                if (result != null)
                {
                    response.Success = true;
                    response.Message = "Answer time stats calculated successfully";
                    response.Data = result;
                }
                else
                {
                    response.Success = false;
                    response.Message = "No data found";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        public async Task<ServiceResponse<AccuracyRateDtoComparison>> GetAccuracyRates(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<AccuracyRateDtoComparison>(true, string.Empty, null, 200);

            try
            {
                using var connection = _connection;

                string sqlQuery = @"
        WITH AccuracyData AS (
            SELECT 
                cpa.StudentId,
                COUNT(cpa.QuestionId) AS TotalAttempts,
                SUM(CASE WHEN cpa.IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectAttempts,
                CASE 
                    WHEN COUNT(cpa.QuestionId) > 0 
                    THEN (SUM(CASE WHEN cpa.IsCorrect = 1 THEN 1 ELSE 0 END) * 100.0) / COUNT(cpa.QuestionId)
                    ELSE 0 
                END AS AccuracyRate
            FROM tblConceptwisePracticeAnswers cpa
            INNER JOIN tblConceptwisePracticeQuestions cpq
                ON cpa.QuestionId = cpq.QuestionId
            WHERE cpq.IndexTypeID = @IndexTypeID
              AND cpq.ContentID = @ContentID
              AND cpq.SyllabusID = @SyllabusID
            GROUP BY cpa.StudentId
        )
        SELECT 
            (SELECT AccuracyRate FROM AccuracyData WHERE StudentId = @StudentId) AS StudentAccuracyRate,
            (SELECT MAX(AccuracyRate) FROM AccuracyData) AS TopperAccuracyRate,
            (SELECT AVG(AccuracyRate) FROM AccuracyData WHERE StudentId <> @StudentId) AS AverageAccuracyOfOthers;";

                var parameters = new
                {
                    StudentId = studentId,
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                };

                var result = await connection.QueryFirstOrDefaultAsync<AccuracyRateDtoComparison>(sqlQuery, parameters);

                if (result != null)
                {
                    response.Success = true;
                    response.Message = "Accuracy rates calculated successfully.";
                    response.Data = result;
                }
                else
                {
                    response.Success = false;
                    response.Message = "No data found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        public async Task<ServiceResponse<List<StudentAccuracyDto>>> GetClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            var response = new ServiceResponse<List<StudentAccuracyDto>>(true, string.Empty, null, 200);

            try
            {
                using var connection = _connection;

                // Fetch the student's class, course, board details along with first and last name
                string studentClassQuery = @"
        SELECT sc.ClassID, sc.CourseID, sc.BoardID, r.FirstName, r.LastName
        FROM tblStudentClassCourseMapping sc
        JOIN tblRegistration r ON r.RegistrationID = sc.RegistrationID
        WHERE sc.RegistrationID = @StudentId;";

                var studentClass = await connection.QueryFirstOrDefaultAsync(studentClassQuery, new { StudentId = studentId });

                if (studentClass == null)
                {
                    response.Success = false;
                    response.Message = "Student details not found.";
                    return response;
                }

                // Query to calculate accuracy rate for the given student
                string accuracyQuery = @"
        WITH QuestionAttempts AS (
            SELECT 
                cpa.QuestionId, 
                cpa.StudentId, 
                cpa.AnswerStatusId
            FROM tblConceptwisePracticeAnswers cpa
            INNER JOIN tblConceptwisePracticeQuestions cpq
                ON cpa.QuestionId = cpq.QuestionId
            WHERE cpq.IndexTypeID = @IndexTypeID
                AND cpq.ContentID = @ContentID
                AND cpq.SyllabusID = @SyllabusID
                AND cpa.StudentId = @StudentId
        )
        SELECT 
            @StudentId AS StudentId,
            COUNT(DISTINCT cpq.QuestionId) AS TotalQuestions,
            COUNT(qa.QuestionId) AS TotalAttempts,
            COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) AS TotalCorrectAttempts,
            (COUNT(CASE WHEN qa.AnswerStatusId = 1 THEN 1 END) * 100.0) / NULLIF(COUNT(qa.QuestionId), 0) AS AccuracyRate
        FROM tblConceptwisePracticeQuestions cpq
        LEFT JOIN QuestionAttempts qa 
            ON cpq.QuestionId = qa.QuestionId
        WHERE cpq.IndexTypeID = @IndexTypeID
            AND cpq.ContentID = @ContentID
            AND cpq.SyllabusID = @SyllabusID;";

                var studentAccuracy = await connection.QueryFirstOrDefaultAsync<StudentAccuracyDto>(accuracyQuery, new
                {
                    StudentId = studentId,
                    IndexTypeID = indexTypeId,
                    ContentID = contentId,
                    SyllabusID = syllabusId
                });

                if (studentAccuracy == null)
                {
                    response.Success = false;
                    response.Message = "No accuracy data found for the student.";
                    return response;
                }

                // Map student first name and last name to the response
                studentAccuracy.FirstName = studentClass.FirstName;
                studentAccuracy.LastName = studentClass.LastName;

                // Fetch classmates (excluding the given student)
                string classmatesQuery = @"
        SELECT r.RegistrationID as StudentId, r.FirstName as FirstName, r.LastName as LastName
        FROM tblStudentClassCourseMapping sc
        JOIN tblRegistration r ON r.RegistrationID = sc.RegistrationID
        WHERE sc.ClassID = @ClassID
            AND sc.CourseID = @CourseID
            AND sc.BoardID = @BoardID
            AND sc.RegistrationID <> @StudentId;";

                var classmates = await connection.QueryAsync<StudentAccuracyDto>(classmatesQuery, new
                {
                    ClassID = studentClass.ClassID,
                    CourseID = studentClass.CourseID,
                    BoardID = studentClass.BoardID,
                    StudentId = studentId
                });

                List<StudentAccuracyDto> result = new List<StudentAccuracyDto> { studentAccuracy };

                // Add accuracy for classmates
                foreach (var classmate in classmates)
                {
                    var classmateAccuracy = await connection.QueryFirstOrDefaultAsync<StudentAccuracyDto>(accuracyQuery, new
                    {
                        StudentId = classmate.StudentId,
                        IndexTypeID = indexTypeId,
                        ContentID = contentId,
                        SyllabusID = syllabusId
                    });

                    if (classmateAccuracy != null)
                    {
                        classmateAccuracy.FirstName = classmate.FirstName;
                        classmateAccuracy.LastName = classmate.LastName;
                        result.Add(classmateAccuracy);
                    }
                }

                // Sort the result list by accuracy rate in descending order, ensuring the student's record is always first
                result = result.OrderByDescending(x => x.AccuracyRate).ToList();
                result.Remove(studentAccuracy);
                result.Insert(0, studentAccuracy);

                response.Data = result;
                response.Success = true;
                response.Message = "Accuracy rates retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }
        private decimal PercentageCalculation(int indexTypeId, int? contentIndexId, int registrationId, int? subjectid, int? SyllabusId)
        {
            if (contentIndexId == null)
                return 0;

            decimal percentage = 0;

            int totalQuestions = 0;
            int markedQuestions = 0;

            if (indexTypeId == 0 && contentIndexId == 0) // New Scenario: Calculate for entire syllabus
            {
                // Step 1: Fetch all chapters in the syllabus
                var chapterIds = _connection.Query<int>(
                    @"SELECT ContentIndexId 
                  FROM tblSyllabusDetails 
                  WHERE IndexTypeId = 1 
                    AND SyllabusID = @SyllabusId 
                    AND SubjectId = @SubjectId",
                    new { SyllabusId = SyllabusId, SubjectId = subjectid }).ToList();

                // Step 2: Fetch all topics belonging to these chapters and present in syllabus details
                var topicIds = _connection.Query<int>(
                    @"SELECT ContentIndexId 
                  FROM tblSyllabusDetails 
                  WHERE IndexTypeId = 2 
                    AND ContentIndexId IN (
                        SELECT ContInIdTopic 
                        FROM tblContentIndexTopics 
                        WHERE ContentIndexId IN @ChapterIds)
                    AND SyllabusID = @SyllabusId 
                    AND SubjectId = @SubjectId",
                    new { ChapterIds = chapterIds, SyllabusId = SyllabusId, SubjectId = subjectid }).ToList();

                // Step 3: Fetch all subtopics belonging to these topics and present in syllabus details
                var subTopicIds = _connection.Query<int>(
                    @"SELECT ContentIndexId 
                  FROM tblSyllabusDetails 
                  WHERE IndexTypeId = 3 
                    AND ContentIndexId IN (
                        SELECT ContInIdSubTopic 
                        FROM tblContentIndexSubTopics 
                        WHERE ContInIdTopic IN @TopicIds 
                          AND IsActive = 1)
                    AND SyllabusID = @SyllabusId 
                    AND SubjectId = @SubjectId",
                    new { TopicIds = topicIds, SyllabusId = SyllabusId, SubjectId = subjectid }).ToList();

                // Step 4: Calculate total questions in chapters, topics, and subtopics
                totalQuestions = _connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) 
                  FROM tblQuestion 
                  WHERE (IndexTypeId = 1 AND ContentIndexId IN @ChapterIds 
                         OR IndexTypeId = 2 AND ContentIndexId IN @TopicIds 
                         OR IndexTypeId = 3 AND ContentIndexId IN @SubTopicIds)
                    AND QuestionTypeId IN (3, 7, 8) 
                    AND IsActive = 1",
                    new { ChapterIds = chapterIds, TopicIds = topicIds, SubTopicIds = subTopicIds });

                // Step 5: Calculate marked questions in chapters, topics, and subtopics
                markedQuestions = _connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) 
                  FROM tblRefresherGuideQuestionRead RGQR
                  INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                  WHERE (Q.IndexTypeId = 1 AND Q.ContentIndexId IN @ChapterIds 
                         OR Q.IndexTypeId = 2 AND Q.ContentIndexId IN @TopicIds 
                         OR Q.IndexTypeId = 3 AND Q.ContentIndexId IN @SubTopicIds)
                    AND RGQR.StudentID = @RegistrationId 
                    AND Q.QuestionTypeId IN (3, 7, 8) 
                    AND Q.IsActive = 1",
                    new { ChapterIds = chapterIds, TopicIds = topicIds, SubTopicIds = subTopicIds, RegistrationId = registrationId });
            }
            if (indexTypeId == 3) // Sub-Concept logic
            {
                totalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = @IndexTypeId
                     AND ContentIndexId = @ContentIndexId
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { IndexTypeId = indexTypeId, ContentIndexId = contentIndexId });

                markedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = @IndexTypeId
                     AND Q.ContentIndexId = @ContentIndexId
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { IndexTypeId = indexTypeId, ContentIndexId = contentIndexId, RegistrationId = registrationId });
            }
            else if (indexTypeId == 2) // Concept logic
            {
                var childSubConceptIds = _connection.Query<int>(
     @"SELECT CIST.ContInIdSubTopic
      FROM tblContentIndexSubTopics CIST
      INNER JOIN tblSyllabusDetails SD
          ON CIST.ContInIdTopic = SD.ContentIndexId
      WHERE CIST.ContInIdTopic = @ContentIndexId
        AND CIST.IsActive = 1
        AND SD.IndexTypeId = @IndexTypeId
        AND SD.SubjectId = @SubjectId
        AND SD.SyllabusID = @SyllabusId
        AND SD.Status = 1",
     new { ContentIndexId = contentIndexId, IndexTypeId = 3, SubjectId = subjectid, SyllabusId = SyllabusId }).ToList();


                int childTotalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = 3
                     AND ContentIndexId IN @ContentIndexIds
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { ContentIndexIds = childSubConceptIds });

                int childMarkedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = 3
                     AND Q.ContentIndexId IN @ContentIndexIds
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { ContentIndexIds = childSubConceptIds, RegistrationId = registrationId });

                int conceptTotalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = 2
                     AND ContentIndexId = @ContentIndexId
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { ContentIndexId = contentIndexId });

                int conceptMarkedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = 2
                     AND Q.ContentIndexId = @ContentIndexId
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { ContentIndexId = contentIndexId, RegistrationId = registrationId });

                totalQuestions = childTotalQuestions + conceptTotalQuestions;
                markedQuestions = childMarkedQuestions + conceptMarkedQuestions;
            }
            else if (indexTypeId == 1) // Chapter logic
            {
                var childTopicIds = _connection.Query<int>(
       @"SELECT CIT.ContInIdTopic
      FROM tblContentIndexTopics CIT
      INNER JOIN tblSyllabusDetails SD
          ON CIT.ContentIndexId = SD.ContentIndexId
      WHERE CIT.ContentIndexId = @ContentIndexId
        AND CIT.IsActive = 1
        AND SD.IndexTypeId = @IndexTypeId
        AND SD.SubjectId = @SubjectId
        AND SD.SyllabusId = @SyllabusId
        AND SD.Status = 1",
       new { ContentIndexId = contentIndexId, IndexTypeId = 2, SubjectId = subjectid, SyllabusId = SyllabusId }).ToList();


                var childSubConceptIds = _connection.Query<int>(
     @"SELECT CST.ContInIdSubTopic
      FROM tblContentIndexSubTopics CST
      INNER JOIN tblSyllabusDetails SD
          ON CST.ContInIdTopic = SD.ContentIndexId
      WHERE CST.ContInIdTopic IN @ChildTopicIds
        AND CST.IsActive = 1
        AND SD.IndexTypeId = @IndexTypeId
        AND SD.SubjectId = @SubjectId
        AND SD.SyllabusId = @SyllabusId
        AND SD.Status = 1",
     new { ChildTopicIds = childTopicIds, IndexTypeId = 3, SubjectId = subjectid, SyllabusId = SyllabusId }).ToList();


                int subConceptTotalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = 3
                     AND ContentIndexId IN @ContentIndexIds
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { ContentIndexIds = childSubConceptIds });

                int subConceptMarkedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = 3
                     AND Q.ContentIndexId IN @ContentIndexIds
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { ContentIndexIds = childSubConceptIds, RegistrationId = registrationId });

                int topicTotalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = 2
                     AND ContentIndexId IN @ContentIndexIds
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { ContentIndexIds = childTopicIds });

                int topicMarkedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = 2
                     AND Q.ContentIndexId IN @ContentIndexIds
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { ContentIndexIds = childTopicIds, RegistrationId = registrationId });

                int chapterTotalQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblQuestion
                   WHERE IndexTypeId = 1
                     AND ContentIndexId = @ContentIndexId
                     AND QuestionTypeId IN (3, 7, 8)
                     AND IsActive = 1",
                    new { ContentIndexId = contentIndexId });

                int chapterMarkedQuestions = _connection.ExecuteScalar<int>(
                    $@"SELECT COUNT(*)
                   FROM tblRefresherGuideQuestionRead RGQR
                   INNER JOIN tblQuestion Q ON RGQR.QuestionID = Q.QuestionId
                   WHERE Q.IndexTypeId = 1
                     AND Q.ContentIndexId = @ContentIndexId
                     AND RGQR.StudentID = @RegistrationId
                     AND Q.QuestionTypeId IN (3, 7, 8)
                     AND Q.IsActive = 1",
                    new { ContentIndexId = contentIndexId, RegistrationId = registrationId });

                totalQuestions = subConceptTotalQuestions + topicTotalQuestions + chapterTotalQuestions;
                markedQuestions = subConceptMarkedQuestions + topicMarkedQuestions + chapterMarkedQuestions;
            }

            // Calculate percentage
            if (totalQuestions > 0)
            {
                percentage = ((decimal)markedQuestions / totalQuestions) * 100;
            }


            return Math.Round(percentage, 2); // Return percentage rounded to 2 decimal places
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
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

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
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMasters>(@"
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
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMasters>(@"
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
        private async Task<ConceptwiseAnswerResponse> HandleSingleAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            // Query the question type for the given QuestionID
            const string answerIdQuery = @"
                SELECT Answerid 
                FROM tblAnswerMaster
                WHERE Questionid = @QuestionID";

            var answerId = await _connection.ExecuteScalarAsync<int>(answerIdQuery, new { request.QuestionID });
            const string singleAnswerQuery = @"
        SELECT Answersingleanswercategoryid 
        FROM tblAnswersingleanswercategory
        WHERE Answerid = @AnswerID ";

            var correctAnswerId = await _connection.ExecuteScalarAsync<int?>(singleAnswerQuery, new
            {
                answerId
            });

            return new ConceptwiseAnswerResponse
            {
                QuestionID = request.QuestionID,
              //  AnswerID = int.Parse(request.AnswerID),
                IsAnswerCorrect = correctAnswerId.HasValue
            };
        }
        private async Task<ConceptwiseAnswerResponse> HandleMultipleChoiceAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {

            // Query the question type for the given QuestionID
            const string answerIdQuery = @"
                SELECT Answerid 
                FROM tblAnswerMaster
                WHERE Questionid = @QuestionID";

            var answerId = await _connection.ExecuteScalarAsync<int>(answerIdQuery, new { request.QuestionID });

            const string multipleChoiceQuery = @"
        SELECT Answermultiplechoicecategoryid
        FROM tblAnswerMultipleChoiceCategory
        WHERE Answerid = @Answerid AND Iscorrect = 1";

            var correctAnswers = (await _connection.QueryAsync<int>(multipleChoiceQuery, new { answerId })).ToList();

            var studentAnswers = request.AnswerID.Split(',')
                                                 .Select(int.Parse)
                                                 .ToList();

            var isCorrect = !correctAnswers.Except(studentAnswers).Any() && !studentAnswers.Except(correctAnswers).Any();

            return new ConceptwiseAnswerResponse
            {
                QuestionID = request.QuestionID,
              //  AnswerID = , // For multiple answers, AnswerID is not relevant
                IsAnswerCorrect = isCorrect
            };
        }
        private async Task<ConceptwiseAnswerResponse> HandleMatchThePairAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            const string matchThePairQuery = @"
        SELECT PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionId = @QuestionID";

            var correctPairs = (await _connection.QueryAsync<string>(matchThePairQuery, new { request.QuestionID })).ToList();

            var studentPairs = request.AnswerID.Split(',')
                                               .ToList();

            var isCorrect = !correctPairs.Except(studentPairs).Any() && !studentPairs.Except(correctPairs).Any();

            return new ConceptwiseAnswerResponse
            {
                QuestionID = request.QuestionID,
               // AnswerID = 0, // Not relevant for pair questions
                IsAnswerCorrect = isCorrect
            };
        }
        private async Task<ConceptwiseAnswerResponse> HandleMatchThePair2Async(ConceptwisePracticeSubmitAnswerRequest request)
        {
            // Query the question type for the given QuestionID
            const string answerIdQuery = @"
                SELECT Answerid 
                FROM tblAnswerMaster
                WHERE Questionid = @QuestionID";

            var answerId = await _connection.ExecuteScalarAsync<int>(answerIdQuery, new { request.QuestionID });
            const string matchThePair2Query = @"
        SELECT MatchThePair2Id
        FROM tblOptionsMatchThePair2
        WHERE AnswerId = @AnswerId";

            var correctAnswers = (await _connection.QueryAsync<int>(matchThePair2Query, new { answerId })).ToList();

            var studentAnswers = request.AnswerID.Split(',')
                                                 .Select(int.Parse)
                                                 .ToList();

            var isCorrect = !correctAnswers.Except(studentAnswers).Any() && !studentAnswers.Except(correctAnswers).Any();

            return new ConceptwiseAnswerResponse
            {
                QuestionID = request.QuestionID,
                //AnswerID = 0, // Not relevant for pair questions
                IsAnswerCorrect = isCorrect
            };
        }
        private async Task<bool> IsPartiallyCorrectAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            // Query to get correct answers
            const string correctAnswersQuery = @"
    SELECT Answermultiplechoicecategoryid 
    FROM tblAnswerMultipleChoiceCategory
    WHERE Answerid = (SELECT Answerid FROM tblAnswerMaster WHERE Questionid = @QuestionID) 
    AND Iscorrect = 1";

            var correctAnswers = (await _connection.QueryAsync<int>(correctAnswersQuery, new { request.QuestionID })).ToList();

            var studentAnswers = request.AnswerID.Split(',')
                                                 .Select(int.Parse)
                                                 .ToList();

            // If some but not all correct answers are selected, it's partially correct
            bool hasSomeCorrect = correctAnswers.Intersect(studentAnswers).Any();
            bool hasSomeIncorrect = studentAnswers.Except(correctAnswers).Any();

            return hasSomeCorrect && hasSomeIncorrect;
        }
    }
}
