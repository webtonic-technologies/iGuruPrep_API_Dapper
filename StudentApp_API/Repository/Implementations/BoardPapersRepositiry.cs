using System.Data;
using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
namespace StudentApp_API.Repository.Implementations
{
    public class BoardPapersRepositiry : IBoardPapersRepository
    {
        private readonly IDbConnection _connection;

        public BoardPapersRepositiry(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId)
        {
            List<TestSeriesSubjectsResponse> response = new List<TestSeriesSubjectsResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch board, class, and course ID using RegistrationId
                string queryStudentMapping = @"
            SELECT CourseID, ClassID, BoardId
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @RegistrationId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync(queryStudentMapping, new { RegistrationId });

                if (studentMapping == null)
                {
                    return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, "No mapping found for the given RegistrationId", null, 404);
                }

                int courseId = studentMapping.CourseID;
                int classId = studentMapping.ClassID;
                int boardId = studentMapping.BoardId;

                // Step 2: Fetch Test Series matching the class, course, and board
                string queryTestSeries = @"
            SELECT ts.TestSeriesId
            FROM tblTestSeries ts
            INNER JOIN tblTestSeriesBoards tsb ON ts.TestSeriesId = tsb.TestSeriesId
            INNER JOIN tblTestSeriesClass tsc ON ts.TestSeriesId = tsc.TestSeriesId
            INNER JOIN tblTestSeriesCourse tscs ON ts.TestSeriesId = tscs.TestSeriesId
            WHERE tsb.BoardId = @BoardId AND tsc.ClassId = @ClassId AND tscs.CourseId = @CourseId AND ts.TypeOfTestSeries = 1002";

                var testSeriesIds = (await _connection.QueryAsync<int>(queryTestSeries, new
                {
                    BoardId = boardId,
                    ClassId = classId,
                    CourseId = courseId
                })).ToList();

                if (!testSeriesIds.Any())
                {
                    return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, "No test series found for the given class, course, and board combination", null, 404);
                }

                // Step 3: Fetch subjects and count of test series grouped by subjects
                string querySubjects = @"
            SELECT s.SubjectId, s.SubjectName, COUNT(tsm.TestSeriesId) AS Count
            FROM tblSubject s
            INNER JOIN tblTestSeriesSubjects tsm ON s.SubjectId = tsm.SubjectId
            WHERE tsm.TestSeriesId IN @TestSeriesIds
            GROUP BY s.SubjectId, s.SubjectName";

                response = (await _connection.QueryAsync<TestSeriesSubjectsResponse>(querySubjects, new
                {
                    TestSeriesIds = testSeriesIds
                })).ToList();
                foreach (var data in response)
                {
                    data.Percentage = CalculatePercentage(RegistrationId, 0, data.SubjectId);
                }
                return new ServiceResponse<List<TestSeriesSubjectsResponse>>(true, "Success", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(GetTestseriesSubjects request)
        {
            List<TestSeriesResponse> response = new List<TestSeriesResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch the TestSeriesIds associated with the given SubjectId
                string queryTestSeriesIds = @"
            SELECT tsm.TestSeriesId
            FROM [tblTestSeriesSubjects] tsm
            WHERE tsm.SubjectId = @SubjectId";

                var testSeriesIds = (await _connection.QueryAsync<int>(queryTestSeriesIds, new { SubjectId = request.SubjectId })).ToList();

                if (!testSeriesIds.Any())
                {
                    return new ServiceResponse<List<TestSeriesResponse>>(false, "No test series found for the given SubjectId", null, 404);
                }

                // Step 2: Fetch details of test series from tblTestSeries using the retrieved TestSeriesIds
                string queryTestSeries = @"
            SELECT ts.TestSeriesId, ts.NameOfExam AS TestPatternName, ts.Duration, ts.StartDate, ts.StartTime, ts.ResultDate, ts.ResultTime, ts.TotalNoOfQuestions
            FROM tblTestSeries ts
            WHERE ts.TestSeriesId IN @TestSeriesIds";

                response = (await _connection.QueryAsync<TestSeriesResponse>(queryTestSeries, new
                {
                    TestSeriesIds = testSeriesIds
                })).ToList();
               
                foreach (var data in response)
                {
                    data.Percentage = CalculatePercentage(request.RegistrationId, data.TestSeriesId, request.SubjectId);
                }
                var paginatedList = response
                  .Skip((request.PageNumber - 1) * request.PageSize)
                  .Take(request.PageSize)
                  .ToList();
                return new ServiceResponse<List<TestSeriesResponse>>(true, "Success", paginatedList, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponse>>(false, ex.Message, null, 500);
            }
        }
//        public async Task<ServiceResponse<List<TestSeriesQuestionsList>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
//        {
//            List<TestSeriesQuestionResponse> response = new List<TestSeriesQuestionResponse>();

//            if (_connection.State != ConnectionState.Open)
//            {
//                _connection.Open();
//            }

//            try
//            {
//                // Step 1: Fetch QuestionIds associated with the TestSeriesId from tbltestseriesQuestions
//                string queryTestSeriesQuestions = @"
//        SELECT tsq.Questionid
//        FROM tbltestseriesQuestions tsq
//        WHERE tsq.TestSeriesid = @TestSeriesId";

//                var questionIds = (await _connection.QueryAsync<int>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

//                if (!questionIds.Any())
//                {
//                    return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No questions found for the given TestSeriesId", null, 404);
//                }

//                // Step 2: Fetch questions from tblQuestion with the given SubjectId and filter for QuestionTypeId
//                string queryDescriptiveQuestions = @"
//SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage, q.DifficultyLevelId, 
//       q.QuestionTypeId, q.Explanation
//FROM tblQuestion q
//WHERE q.QuestionId IN @QuestionIds
//  AND q.SubjectID = @SubjectId
//  AND (@QuestionTypeId IS NULL OR q.QuestionTypeId IN @QuestionTypeId)";

//                var questions = (await _connection.QueryAsync<TestSeriesQuestionResponse>(queryDescriptiveQuestions, new
//                {
//                    QuestionIds = questionIds,
//                    SubjectId = request.SubjectId,
//                    QuestionTypeId = request.QuestionTypeId?.Count > 0 ? request.QuestionTypeId : null // Pass null if no IDs are specified
//                })).ToList();

//                if (!questions.Any())
//                {
//                    return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No descriptive questions found for the given criteria", null, 404);
//                }

//                // Step 3: Fetch the answers for each question from tblAnswerMaster and tblAnswersingleanswercategory
//                foreach (var question in questions)
//                {
//                    string queryAnswers = @"
//            SELECT am.Answerid, sac.Answer
//            FROM tblAnswerMaster am
//            INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
//            WHERE am.Questionid = @QuestionId";

//                    var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });

//                    question.Answers = answers.ToList();
//                }

//                // Paginate the results
//                var paginatedList = questions
//                    .Skip((request.PageNumber - 1) * request.PageSize)
//                    .Take(request.PageSize)
//                    .ToList();

//                return new ServiceResponse<List<TestSeriesQuestionResponse>>(true, "Success", paginatedList, 200);
//            }
//            catch (Exception ex)
//            {
//                return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, ex.Message, null, 500);
//            }
//        }
        public async Task<ServiceResponse<List<TestSeriesQuestionsList>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch Sections associated with the TestSeriesId
                string queryTestSeriesSections = @"
        SELECT tsqs.testseriesQuestionSectionid, tsqs.TestSeriesid, tsqs.SectionName, tsqs.SubjectId
        FROM tbltestseriesQuestionSection tsqs
        WHERE tsqs.TestSeriesid = @TestSeriesId";

                var sections = (await _connection.QueryAsync<TestSeriesQuestionsList>(queryTestSeriesSections, new { TestSeriesId = request.TestSeriesId })).ToList();

                if (!sections.Any())
                {
                    return new ServiceResponse<List<TestSeriesQuestionsList>>(false, "No sections found for the given TestSeriesId", null, 404);
                }

                // Step 2: Fetch QuestionIds associated with each Section
                string queryTestSeriesQuestions = @"
        SELECT tsq.Questionid, tsq.testseriesQuestionSectionid
        FROM tbltestseriesQuestions tsq
        WHERE tsq.TestSeriesid = @TestSeriesId";

                var allQuestions = (await _connection.QueryAsync<TestSeriesQuestionMapping>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

                // Step 3: Fetch Descriptive Questions for each Section
                string queryDescriptiveQuestions = @"
        SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage,q.QuestionCode, q.DifficultyLevelId, 
               q.QuestionTypeId, q.Explanation
        FROM tblQuestion q
        WHERE q.QuestionId IN @QuestionIds
          AND q.SubjectID = @SubjectId
          AND (@QuestionTypeId IS NULL OR q.QuestionTypeId IN @QuestionTypeId)";

                foreach (var section in sections)
                {
                    var questionIdsInSection = allQuestions
                        .Where(q => q.testseriesQuestionSectionid == section.testseriesQuestionSectionid)
                        .Select(q => q.Questionid)
                        .ToList();

                    if (!questionIdsInSection.Any()) continue;

                    var questions = (await _connection.QueryAsync<TestSeriesQuestionResponse>(queryDescriptiveQuestions, new
                    {
                        QuestionIds = questionIdsInSection,
                        SubjectId = section.SubjectId,
                        QuestionTypeId = request.QuestionTypeId?.Count > 0 ? request.QuestionTypeId : null
                    })).ToList();

                    // Step 4: Fetch Answers for each question
                    foreach (var question in questions)
                    {
                        string queryAnswers = @"
                SELECT am.Answerid, sac.Answer
                FROM tblAnswerMaster am
                INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
                WHERE am.Questionid = @QuestionId";

                        var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });

                        question.Answers = answers.ToList();
                    }

                    // Assign the questions to the section
                    section.TestSeriesQuestionResponses = questions;
                }

                // Filter and paginate the sections that contain questions
                var paginatedSections = sections
                    .Where(s => s.TestSeriesQuestionResponses != null && s.TestSeriesQuestionResponses.Any())
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (!paginatedSections.Any())
                {
                    return new ServiceResponse<List<TestSeriesQuestionsList>>(false, "No descriptive questions found for the given criteria", null, 404);
                }

                return new ServiceResponse<List<TestSeriesQuestionsList>>(true, "Success", paginatedSections, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesQuestionsList>>(false, ex.Message, null, 500);
            }
        }

        //public async Task<ServiceResponse<List<TestSeriesQuestionResponse>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        //{
        //    List<TestSeriesQuestionResponse> response = new List<TestSeriesQuestionResponse>();

        //    if (_connection.State != ConnectionState.Open)
        //    {
        //        _connection.Open();
        //    }

        //    try
        //    {
        //        // Step 1: Fetch QuestionIds associated with the TestSeriesId from tbltestseriesQuestions
        //        string queryTestSeriesQuestions = @"
        //    SELECT tsq.Questionid
        //    FROM tbltestseriesQuestions tsq
        //    WHERE tsq.TestSeriesid = @TestSeriesId";

        //        var questionIds = (await _connection.QueryAsync<int>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

        //        if (!questionIds.Any())
        //        {
        //            return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No questions found for the given TestSeriesId", null, 404);
        //        }

        //        // Step 2: Fetch questions from tblQuestion associated with the SubjectId and filter for descriptive types (QuestionTypeId IN (3, 7, 8))
        //        string queryDescriptiveQuestions = @"
        //    SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage, q.DifficultyLevelId, 
        //           q.QuestionTypeId, q.Explanation
        //    FROM tblQuestion q
        //    WHERE q.QuestionId IN @QuestionIds
        //      AND q.SubjectID = @SubjectId
        //      AND q.QuestionTypeId IN (3, 7, 8) -- SA, LA, VSA";

        //        var questions = (await _connection.QueryAsync<TestSeriesQuestionResponse>(queryDescriptiveQuestions, new
        //        {
        //            QuestionIds = questionIds,
        //            SubjectId = request.SubjectId
        //        })).ToList();

        //        if (!questions.Any())
        //        {
        //            return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No descriptive questions found for the given criteria", null, 404);
        //        }

        //        // Step 3: Fetch the answers for each question from tblAnswerMaster and tblAnswersingleanswercategory
        //        foreach (var question in questions)
        //        {
        //            string queryAnswers = @"
        //        SELECT am.Answerid, sac.Answer
        //        FROM tblAnswerMaster am
        //        INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
        //        WHERE am.Questionid = @QuestionId";

        //            var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });

        //            question.Answers = answers.ToList();
        //        }
        //        var paginatedList = questions
        //         .Skip((request.PageNumber - 1) * request.PageSize)
        //         .Take(request.PageSize)
        //         .ToList();
        //        return new ServiceResponse<List<TestSeriesQuestionResponse>>(true, "Success", paginatedList, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, ex.Message, null, 500);
        //    }
        //}
        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request)
        {
            try
            {
                // Check if the record already exists
                string checkQuery = @"
            SELECT COUNT(1)
            FROM tblBoardPaperQuestionRead
            WHERE StudentID = @RegistrationId
              AND QuestionID = @QuestionId
              AND QuestionCode = @QuestionCode";

                int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    request.RegistrationId,
                    request.QuestionId,
                    request.QuestionCode
                });

                if (recordExists > 0)
                {
                    // If record exists, delete it
                    string deleteQuery = @"
                DELETE FROM tblBoardPaperQuestionRead
                WHERE StudentID = @RegistrationId
                  AND QuestionID = @QuestionId
                  AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question marked as unread (deleted).", null, 200);
                }
                else
                {
                    // If record does not exist, insert it
                    string insertQuery = @"
                INSERT INTO tblBoardPaperQuestionRead (StudentID, QuestionID, QuestionCode, SubjectId, TestSeriesId)
                VALUES (@RegistrationId, @QuestionId, @QuestionCode, @SubjectId, @TestSeriesId)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode,
                        request.SubjectId,
                        request.TestSeriesId
                    });

                    return new ServiceResponse<string>(true, "Question marked as read (inserted).", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"An error occurred: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            try
            {
                // Check if the record already exists
                string checkQuery = @"
            SELECT COUNT(1)
            FROM tblBoardPaperQuestionSave
            WHERE StudentID = @RegistrationId
              AND QuestionID = @QuestionId
              AND QuestionCode = @QuestionCode";

                int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    request.RegistrationId,
                    request.QuestionId,
                    request.QuestionCode
                });

                if (recordExists > 0)
                {
                    // If record exists, delete it
                    string deleteQuery = @"
                DELETE FROM tblBoardPaperQuestionSave
                WHERE StudentID = @RegistrationId
                  AND QuestionID = @QuestionId
                  AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question unsaved (deleted).", null, 200);
                }
                else
                {
                    // If record does not exist, insert it
                    string insertQuery = @"
                INSERT INTO tblBoardPaperQuestionSave (StudentID, QuestionID, QuestionCode,SubjectId, TestSeriesId)
                VALUES (@RegistrationId, @QuestionId, @QuestionCode,@SubjectId, @TestSeriesId)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode,
                        request.SubjectId,
                        request.TestSeriesId
                    });

                    return new ServiceResponse<string>(true, "Question saved (inserted).", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"An error occurred: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByTestSeriesIdAsync(int testSeriesId)
        {
            try
            {
                // SQL query to fetch question types
                string query = @"
            SELECT DISTINCT 
                qt.QuestionTypeID,
                qt.QuestionType
            FROM tbltestseriesQuestionSection ts
            INNER JOIN tblQBQuestionType qt ON ts.QuestionTypeID = qt.QuestionTypeID
            WHERE ts.TestSeriesId = @TestSeriesId
            ORDER BY qt.QuestionTypeID;
        ";

                // Execute query and fetch results
                var questionTypes = await _connection.QueryAsync<QuestionTypeResponse>(
                    query,
                    new { TestSeriesId = testSeriesId }
                );

                // Check if data exists
                if (questionTypes.Any())
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(
                        true,
                        "Question types retrieved successfully.",
                        questionTypes.ToList(),
                        200
                    );
                }

                return new ServiceResponse<List<QuestionTypeResponse>>(
                    false,
                    "No question types found for the given test series ID.",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeResponse>>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<Dictionary<string, object>>> GetTestSeriesPercentageBySubject(int RegistrationId)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch board, class, and course ID using RegistrationId
                string queryStudentMapping = @"
            SELECT CourseID, ClassID, BoardId
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @RegistrationId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync(queryStudentMapping, new { RegistrationId });

                if (studentMapping == null)
                {
                    return new ServiceResponse<Dictionary<string, object>>(false, "No mapping found for the given RegistrationId", null, 404);
                }

                int courseId = studentMapping.CourseID;
                int classId = studentMapping.ClassID;
                int boardId = studentMapping.BoardId;

                // Step 2: Fetch Test Series matching the class, course, and board
                string queryTestSeries = @"
            SELECT ts.TestSeriesId, s.SubjectId, s.SubjectName
            FROM tblTestSeries ts
            INNER JOIN tblTestSeriesSubjects tsm ON ts.TestSeriesId = tsm.TestSeriesId
            INNER JOIN tblSubject s ON s.SubjectId = tsm.SubjectId
            INNER JOIN tblTestSeriesBoards tsb ON ts.TestSeriesId = tsb.TestSeriesId
            INNER JOIN tblTestSeriesClass tsc ON ts.TestSeriesId = tsc.TestSeriesId
            INNER JOIN tblTestSeriesCourse tscs ON ts.TestSeriesId = tscs.TestSeriesId
            WHERE tsb.BoardId = @BoardId AND tsc.ClassId = @ClassId AND tscs.CourseId = @CourseId
            AND ts.TypeOfTestSeries = 1002";

                var testSeriesSubjects = await _connection.QueryAsync<TestSeriesSubjectDetails>(queryTestSeries, new
                {
                    BoardId = boardId,
                    ClassId = classId,
                    CourseId = courseId
                });

                if (!testSeriesSubjects.Any())
                {
                    return new ServiceResponse<Dictionary<string, object>>(false, "No test series found for the given class, course, and board combination", null, 404);
                }

                // Step 3: Prepare a structure to hold results
                var subjectPercentages = new Dictionary<int, Dictionary<string, object>>();

                // Step 4: Loop through each subject's test series to calculate the percentage
                foreach (var testSeriesSubject in testSeriesSubjects)
                {
                    // Fetch the bookmarked questions for each test series
                    string queryBookmarkedQuestions = @"
                SELECT COUNT(*) AS BookmarkedCount
                FROM tblBoardPaperQuestionRead bpq
                INNER JOIN tblTestSeriesQuestions tsq ON bpq.QuestionID = tsq.QuestionID
                WHERE tsq.TestSeriesId = @TestSeriesId AND bpq.StudentID = @StudentID";

                    var bookmarkedQuestionCount = await _connection.QueryFirstOrDefaultAsync<int>(queryBookmarkedQuestions, new
                    {
                        TestSeriesId = testSeriesSubject.TestSeriesId,
                        StudentID = RegistrationId  // Assuming RegistrationId can be used to identify the student
                    });

                    // Calculate the total questions for the test series (assumed to be all questions in the test series)
                    string queryTotalQuestions = @"
                SELECT COUNT(*) AS TotalQuestions
                FROM tblTestSeriesQuestions tsq
                WHERE tsq.TestSeriesId = @TestSeriesId";

                    var totalQuestionCount = await _connection.QueryFirstOrDefaultAsync<int>(queryTotalQuestions, new { TestSeriesId = testSeriesSubject.TestSeriesId });

                    // Calculate the percentage based on bookmarked questions
                    double percentage = (totalQuestionCount > 0) ? ((double)bookmarkedQuestionCount / totalQuestionCount) * 100 : 0;

                    // Add the percentage for the test series to the subject dictionary
                    if (!subjectPercentages.ContainsKey(testSeriesSubject.SubjectId))
                    {
                        subjectPercentages[testSeriesSubject.SubjectId] = new Dictionary<string, object>();
                    }

                    var subjectData = subjectPercentages[testSeriesSubject.SubjectId];
                    subjectData.Add(testSeriesSubject.SubjectName + " - TestSeries " + testSeriesSubject.TestSeriesId, Math.Round(percentage, 2));
                }

                // Step 5: Calculate the average percentage for each subject
                foreach (var subject in subjectPercentages)
                {
                    var testSeriesPercentages = subject.Value.Values.Cast<double>().ToList();
                    double subjectAveragePercentage = testSeriesPercentages.Any() ? testSeriesPercentages.Average() : 0;

                    subject.Value.Add("SubjectPercentage", Math.Round(subjectAveragePercentage, 2));
                }

                // Convert dictionary to desired response format
                foreach (var subject in subjectPercentages)
                {
                    string subjectName = subject.Value.Keys.First().ToString().Split('-').First().Trim();
                    response.Add(subjectName, subject.Value);
                }

                return new ServiceResponse<Dictionary<string, object>>(true, "Success", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Dictionary<string, object>>(false, ex.Message, null, 500);
            }
        }
        private decimal CalculatePercentage(int StudentId, int TestSeriesId, int SubjectId)
        {
            if (TestSeriesId > 0)
            {
                // Case 1: TestSeriesId is provided
                // Fetch bookmarked question count
                string queryBookmarkedQuestions = @"
            SELECT COUNT(*) AS BookmarkedCount
            FROM tblBoardPaperQuestionRead bpq
            INNER JOIN tblTestSeriesQuestions tsq ON bpq.QuestionID = tsq.QuestionID
            WHERE tsq.TestSeriesId = @TestSeriesId AND bpq.StudentID = @StudentID";

                var bookmarkedQuestionCount = _connection.QueryFirstOrDefault<int>(queryBookmarkedQuestions, new
                {
                    TestSeriesId = TestSeriesId,
                    StudentID = StudentId
                });

                string queryTotalQuestions = @"
SELECT COUNT(*) AS TotalQuestions
FROM tblTestSeriesQuestions tsq
JOIN tblQuestion q ON tsq.Questionid = q.QuestionId
WHERE tsq.TestSeriesId = @TestSeriesId AND q.SubjectId = @SubjectId";

                var totalQuestionCount = _connection.QueryFirstOrDefault<int>(queryTotalQuestions, new
                {
                    TestSeriesId = TestSeriesId,
                    SubjectId = SubjectId
                });

                // Calculate percentage
                if (totalQuestionCount == 0) return 0;

                decimal percentage = ((decimal)bookmarkedQuestionCount / totalQuestionCount) * 100;
                return Math.Round(percentage, 2);
            }
            else
            {
                // Case 2: TestSeriesId is not provided (Subject-based percentage calculation)
                // Fetch the TestSeriesIds associated with the given SubjectId
                string queryTestSeriesIds = @"
            SELECT DISTINCT tsm.TestSeriesId
            FROM tblTestSeriesSubjects tsm
            WHERE tsm.SubjectId = @SubjectId";

                var testSeriesIds = _connection.Query<int>(queryTestSeriesIds, new { SubjectId = SubjectId }).ToList();

                if (!testSeriesIds.Any()) return 0; // No test series associated with the subject

                // Calculate percentage for each test series
                decimal totalPercentage = 0;
                int count = 0;

                foreach (var testSeriesId in testSeriesIds)
                {
                    // Fetch bookmarked question count for the current test series
                    string queryBookmarkedQuestions = @"
                SELECT COUNT(*) AS BookmarkedCount
                FROM tblBoardPaperQuestionRead bpq
                INNER JOIN tblTestSeriesQuestions tsq ON bpq.QuestionID = tsq.QuestionID
                WHERE tsq.TestSeriesId = @TestSeriesId AND bpq.StudentID = @StudentID";

                    var bookmarkedQuestionCount = _connection.QueryFirstOrDefault<int>(queryBookmarkedQuestions, new
                    {
                        TestSeriesId = testSeriesId,
                        StudentID = StudentId
                    });

                    // Fetch total question count for the current test series
                    string queryTotalQuestions = @"
SELECT COUNT(*) AS TotalQuestions
FROM tblTestSeriesQuestions tsq
JOIN tblQuestion q ON tsq.Questionid = q.QuestionId
WHERE tsq.TestSeriesId = @TestSeriesId AND q.SubjectId = @SubjectId";

                    var totalQuestionCount = _connection.QueryFirstOrDefault<int>(queryTotalQuestions, new
                    {
                        TestSeriesId = testSeriesId,
                        SubjectId = SubjectId
                    });

                    if (totalQuestionCount > 0)
                    {
                        decimal percentage = ((decimal)bookmarkedQuestionCount / totalQuestionCount) * 100;
                        totalPercentage += Math.Round(percentage, 2);
                        count++;
                    }
                }

                if (count == 0) return 0;

                // Calculate and return average percentage
                return Math.Round(totalPercentage / count, 2);
            }
        }
    }
}
