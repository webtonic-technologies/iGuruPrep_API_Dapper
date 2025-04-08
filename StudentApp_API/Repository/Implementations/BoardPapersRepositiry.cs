using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
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
            WHERE tsb.BoardId = @BoardId AND tsc.ClassId = @ClassId AND tscs.CourseId = @CourseId AND ts.TypeOfTestSeries = 4";

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
            SELECT ts.TestSeriesId, ts.NameOfExam AS TestPatternName, ts.Duration, ts.StartDate, ts.StartTime, 
            ts.ResultDate, ts.ResultTime, ts.TotalNoOfQuestions
            FROM tblTestSeries ts
            WHERE ts.TestSeriesId IN @TestSeriesIds AND ts.DownloadStatusId >= 3 AND ts.TypeOfTestSeries = 4";

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
                return new ServiceResponse<List<TestSeriesResponse>>(true, "Success", paginatedList, 200, response.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<TestSeriesQuestionsListResponse>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch Sections associated with the TestSeriesId
                string queryTestSeriesSections = @"
 SELECT tsqs.testseriesQuestionSectionid, tsqs.TestSeriesid, tsqs.SectionName as SectionName, tsqs.SubjectId
 FROM tbltestseriesQuestionSection tsqs
 WHERE tsqs.TestSeriesid = @TestSeriesId";

                var sections = (await _connection.QueryAsync<TestSeriesQuestionsList>(queryTestSeriesSections, new { TestSeriesId = request.TestSeriesId })).ToList();

                if (!sections.Any())
                {
                    return new ServiceResponse<TestSeriesQuestionsListResponse>(false, "No sections found for the given TestSeriesId", null, 404);
                }

                // Step 2: Fetch QuestionIds associated with each Section
                string queryTestSeriesQuestions = @"
 SELECT tsq.Questionid, tsq.testseriesQuestionSectionid
 FROM tbltestseriesQuestions tsq
 WHERE tsq.TestSeriesid = @TestSeriesId";

                var allQuestions = (await _connection.QueryAsync<TestSeriesQuestionMapping>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

                // Step 3: Fetch descriptive questions
                string queryDescriptiveQuestions = @"
 SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage, q.QuestionCode, q.DifficultyLevelId, 
        q.QuestionTypeId, q.Explanation
 FROM tblQuestion q
 WHERE q.QuestionId IN @QuestionIds
   AND q.SubjectID = @SubjectId
   AND q.IsActive = 1 and q.IsLive = 1";

                if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                {
                    queryDescriptiveQuestions += " AND q.QuestionTypeId IN @QuestionTypeId";
                }

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

                    // Step 4: Check question status and filter results
                    var filteredQuestions = new List<TestSeriesQuestionResponse>();

                    if (request.QuestionStatus != null && request.QuestionStatus.Any())
                    {
                        foreach (var question in questions)
                        {
                            // Initialize a flag to track if the question exists in any of the tables
                            bool isPresentInAnyTable = false;

                            // Check if the question is in 'Saved' table
                            if (request.QuestionStatus.Contains(16)) // Save
                            {
                                var isSaved = await _connection.ExecuteScalarAsync<bool>(
                                    "SELECT COUNT(1) FROM tblBoardPaperQuestionSave WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
                                    new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
                                if (isSaved)
                                {
                                    // question.Status = "Saved"; // Update status if necessary
                                    isPresentInAnyTable = true;
                                }
                            }

                            // Check if the question is in 'Read' table
                            if (request.QuestionStatus.Contains(17)) // Mark as Read
                            {
                                var isRead = await _connection.ExecuteScalarAsync<bool>(
                                    "SELECT COUNT(1) FROM tblBoardPaperQuestionRead WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
                                    new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
                                if (isRead)
                                {
                                    //  question.Status = "Read"; // Update status if necessary
                                    isPresentInAnyTable = true;
                                }
                            }

                            // Check if the question is in 'Reposted' table
                            if (request.QuestionStatus.Contains(18)) // Repost
                            {
                                var isReposted = await _connection.ExecuteScalarAsync<bool>(
                                    "SELECT COUNT(1) FROM tblReportedQuestions WHERE QuestionID = @QuestionId AND SubjectID = @SubjectId",
                                    new { QuestionId = question.QuestionId, SubjectId = request.SubjectId });
                                if (isReposted)
                                {
                                    // question.Status = "Reposted"; // Update status if necessary
                                    isPresentInAnyTable = true;
                                }
                            }

                            // Add to filtered list if present in any table
                            if (isPresentInAnyTable)
                            {
                                filteredQuestions.Add(question);
                            }
                        }
                    }
                    else
                    {
                        filteredQuestions = questions;
                    }
                    // Step 5: Fetch answers for each filtered question
                    foreach (var question in filteredQuestions)
                    {
                        string queryAnswers = @"
         SELECT am.Answerid, sac.Answer
         FROM tblAnswerMaster am
         INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
         WHERE am.Questionid = @QuestionId";

                        var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });
                        question.Answers = answers.ToList();
                    }

                    // Assign filtered questions to the section
                    section.TestSeriesQuestionResponses = filteredQuestions;
                }

                // Filter and paginate the sections that contain questions
                var paginatedSections = sections
                    .Where(s => s.TestSeriesQuestionResponses != null && s.TestSeriesQuestionResponses.Any())
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (!paginatedSections.Any())
                {
                    return new ServiceResponse<TestSeriesQuestionsListResponse>(false, "No descriptive questions found for the given criteria", null, 404);
                }

                // Map the paginated sections into the response object
                var responseData = new TestSeriesQuestionsListResponse
                {
                    TestSeriesId = request.TestSeriesId,
                    TestSeriesQuestionsLists = paginatedSections
                };

                return new ServiceResponse<TestSeriesQuestionsListResponse>(true, "Success", responseData, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestSeriesQuestionsListResponse>(false, ex.Message, null, 500);
            }
        }
        //public async Task<ServiceResponse<TestSeriesQuestionsListResponse>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        //{
        //    if (_connection.State != ConnectionState.Open)
        //    {
        //        _connection.Open();
        //    }

        //    try
        //    {
        //        // Step 1: Fetch Sections associated with the TestSeriesId
        //        string queryTestSeriesSections = @"
        //SELECT tsqs.testseriesQuestionSectionid, tsqs.TestSeriesid, tsqs.SectionName as SectionName, tsqs.SubjectId
        //FROM tbltestseriesQuestionSection tsqs
        //WHERE tsqs.TestSeriesid = @TestSeriesId";

        //        var sections = (await _connection.QueryAsync<TestSeriesQuestionsList>(queryTestSeriesSections, new { TestSeriesId = request.TestSeriesId })).ToList();

        //        if (!sections.Any())
        //        {
        //            return new ServiceResponse<TestSeriesQuestionsListResponse>(false, "No sections found for the given TestSeriesId", null, 404);
        //        }

        //        // Step 2: Fetch QuestionIds associated with each Section
        //        string queryTestSeriesQuestions = @"
        //SELECT tsq.Questionid, tsq.testseriesQuestionSectionid
        //FROM tbltestseriesQuestions tsq
        //WHERE tsq.TestSeriesid = @TestSeriesId";

        //        var allQuestions = (await _connection.QueryAsync<TestSeriesQuestionMapping>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

        //        // Step 3: Fetch descriptive questions
        //        string queryDescriptiveQuestions = @"
        //SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage, q.QuestionCode, q.DifficultyLevelId, 
        //       q.QuestionTypeId, q.Explanation
        //FROM tblQuestion q
        //WHERE q.QuestionId IN @QuestionIds
        //  AND q.SubjectID = @SubjectId
        //  AND q.IsActive = 1 and q.IsLive = 1";

        //        if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
        //        {
        //            queryDescriptiveQuestions += " AND q.QuestionTypeId IN @QuestionTypeId";
        //        }

        //        foreach (var section in sections)
        //        {
        //            var questionIdsInSection = allQuestions
        //                .Where(q => q.testseriesQuestionSectionid == section.testseriesQuestionSectionid)
        //                .Select(q => q.Questionid)
        //                .ToList();

        //            if (!questionIdsInSection.Any()) continue;

        //            var questions = (await _connection.QueryAsync<TestSeriesQuestionResponse>(queryDescriptiveQuestions, new
        //            {
        //                QuestionIds = questionIdsInSection,
        //                SubjectId = section.SubjectId,
        //                QuestionTypeId = request.QuestionTypeId?.Count > 0 ? request.QuestionTypeId : null
        //            })).ToList();

        //            // Step 4: Check question status and filter results
        //            var filteredQuestions = new List<TestSeriesQuestionResponse>();

        //            if (request.QuestionStatus != null && request.QuestionStatus.Any())
        //            {
        //                foreach (var question in questions)
        //                {
        //                    // Initialize a flag to track if the question exists in any of the tables
        //                    bool isPresentInAnyTable = false;

        //                    // Check if the question is in 'Saved' table
        //                    if (request.QuestionStatus.Contains(16)) // Save
        //                    {
        //                        var isSaved = await _connection.ExecuteScalarAsync<bool>(
        //                            "SELECT COUNT(1) FROM tblBoardPaperQuestionSave WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
        //                            new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
        //                        if (isSaved)
        //                        {
        //                            // question.Status = "Saved"; // Update status if necessary
        //                            isPresentInAnyTable = true;
        //                        }
        //                    }

        //                    // Check if the question is in 'Read' table
        //                    if (request.QuestionStatus.Contains(17)) // Mark as Read
        //                    {
        //                        var isRead = await _connection.ExecuteScalarAsync<bool>(
        //                            "SELECT COUNT(1) FROM tblBoardPaperQuestionRead WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
        //                            new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
        //                        if (isRead)
        //                        {
        //                            //  question.Status = "Read"; // Update status if necessary
        //                            isPresentInAnyTable = true;
        //                        }
        //                    }

        //                    // Check if the question is in 'Reposted' table
        //                    if (request.QuestionStatus.Contains(18)) // Repost
        //                    {
        //                        var isReposted = await _connection.ExecuteScalarAsync<bool>(
        //                            "SELECT COUNT(1) FROM tblReportedQuestions WHERE QuestionID = @QuestionId AND SubjectID = @SubjectId",
        //                            new { QuestionId = question.QuestionId, SubjectId = request.SubjectId });
        //                        if (isReposted)
        //                        {
        //                            // question.Status = "Reposted"; // Update status if necessary
        //                            isPresentInAnyTable = true;
        //                        }
        //                    }

        //                    // Add to filtered list if present in any table
        //                    if (isPresentInAnyTable)
        //                    {
        //                        filteredQuestions.Add(question);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                filteredQuestions = questions;
        //            }
        //            // Step 5: Fetch answers for each filtered question
        //            foreach (var question in filteredQuestions)
        //            {
        //                string queryAnswers = @"
        //        SELECT am.Answerid, sac.Answer
        //        FROM tblAnswerMaster am
        //        INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
        //        WHERE am.Questionid = @QuestionId";

        //                var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });
        //                question.Answers = answers.ToList();
        //            }

        //            // Assign filtered questions to the section
        //            section.TestSeriesQuestionResponses = filteredQuestions;
        //        }

        //        // Filter and paginate the sections that contain questions
        //        var paginatedSections = sections
        //            .Where(s => s.TestSeriesQuestionResponses != null && s.TestSeriesQuestionResponses.Any())
        //            .Skip((request.PageNumber - 1) * request.PageSize)
        //            .Take(request.PageSize)
        //            .ToList();

        //        if (!paginatedSections.Any())
        //        {
        //            return new ServiceResponse<TestSeriesQuestionsListResponse>(false, "No descriptive questions found for the given criteria", null, 404);
        //        }

        //        return new ServiceResponse<TestSeriesQuestionsListResponse>(true, "Success", paginatedSections, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<TestSeriesQuestionsListResponse>(false, ex.Message, null, 500);
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
        public async Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int TestSeriesId)
        {
            try
            {
                // Step 1: Fetch board, class, and course details for the given student.
                string mappingQuery = @"
            SELECT BoardId, ClassID, CourseID
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @StudentId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    mappingQuery, new { StudentId = studentId });

                if (studentMapping == null)
                {
                    return new ServiceResponse<string>(false, "Student mapping not found.", string.Empty, 404);
                }

                int boardId = studentMapping.BoardId;
                int classId = studentMapping.ClassID;
                int courseId = studentMapping.CourseID;

                // Step 2: Find classmates (students with the same board, class, and course, excluding the given student)
                string classmatesQuery = @"
            SELECT RegistrationID
            FROM tblStudentClassCourseMapping
            WHERE BoardId = @BoardId
              AND ClassID = @ClassID
              AND CourseID = @CourseID
              AND RegistrationID <> @StudentId";

                var classmates = (await _connection.QueryAsync<int>(classmatesQuery, new
                {
                    BoardId = boardId,
                    ClassID = classId,
                    CourseID = courseId,
                    StudentId = studentId
                })).ToList();

                if (classmates == null || !classmates.Any())
                {
                    return new ServiceResponse<string>(false, "No classmates found.", string.Empty, 404);
                }

                // Step 3: Insert a shared question record for each classmate
                string insertQuery = @"
            INSERT INTO tblBoardPaperSharedQuestions (TestSeriesId, QuestionId, SharedBy, SharedTo)
            VALUES (@TestSeriesId, @QuestionId, @SharedBy, @SharedTo)";

                int totalInserted = 0;
                foreach (var classmateId in classmates)
                {
                    int rows = await _connection.ExecuteAsync(insertQuery, new
                    {
                        TestSeriesId = TestSeriesId,
                        QuestionId = questionId,
                        SharedBy = studentId,
                        SharedTo = classmateId
                    });
                    totalInserted += rows;
                }

                var data = GetQuestionById(questionId);

                return new ServiceResponse<string>(true, "Question shared successfully.", $"Shared with {classmates.Count} classmates", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
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
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

        }
        private List<DTOs.Response.MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
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
                return new List<DTOs.Response.MatchThePairAnswer>();
            }

            return _connection.Query<DTOs.Response.MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();

        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<StudentApp_API.Models.AnswerMaster>(@"
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