using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class RefresherGuideRepository : IRefresherGuideRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public RefresherGuideRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<List<QuestionStatusName>>> GetQuestionStatusList()
        {
            var data = await _connection.QueryAsync<QuestionStatusName>(@"select RQSID as StatusId, RQSName as StatusName from tblStatus where 16 <= RQSID and RQSID <= 18");
            return new ServiceResponse<List<QuestionStatusName>>(true, "Records found", data.ToList(), 200);
        }
        public async Task<ServiceResponse<RefresherGuideSubjects>> GetSyllabusSubjects(RefresherGuideRequest request)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync(@"select * from tblStudentClassCourseMapping where RegistrationID = 
                @RegistrationID", new { RegistrationID = request.RegistrationId });
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
                var subjects = resposne.Select(m => new RefresherGuideSubjectsResposne
                {
                    SubjectId = m.SubjectId,
                    SubjectName = m.SubjectName,
                    SyllabusId = m.SyllabusId,
                    RegistrationId = request.RegistrationId,
                    Percentage = PercentageCalculation(0, 0, request.RegistrationId, m.SubjectId, m.SyllabusId)
                }).ToList();
                // Calculate the average percentage
                decimal averagePercentage = subjects.Any()
                    ? subjects.Average(s => s.Percentage)
                    : 0;
                var response = new RefresherGuideSubjects { 
                refresherGuideSubjectsResposnes = [.. subjects],
                Percentage = Math.Round(averagePercentage, 2)
                };
                // Check if any subjects were found
                if (subjects != null && subjects.Count != 0)
                {
                    return new ServiceResponse<RefresherGuideSubjects>(true, "Subjects retrieved successfully.", response, 200);
                }
                else
                {
                    return new ServiceResponse<RefresherGuideSubjects>(false, "No subjects found for the given criteria.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RefresherGuideSubjects>(false, ex.Message, null, 500);
            }
        }
        //public async Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContent(GetContentRequest request)
        //{
        //    try
        //    {
        //        // SQL query to fetch ContentId, SubjectId, IndexTypeId, and SyllabusID from SyllabusDetails
        //        string query = @"
        //SELECT 
        //    sd.ContentIndexId AS ContentId,
        //    sd.SubjectId,
        //    sd.IndexTypeId,
        //    sd.SyllabusID,
        //    sd.Synopsis,
        //FROM tblSyllabusDetails sd
        //WHERE sd.SyllabusID = @SyllabusId AND sd.SubjectId = @SubjectId";

        //        // Fetch syllabus content details
        //        var contentList = await _connection.QueryAsync<dynamic>(query, new { request.SyllabusId, request.SubjectId });

        //        // List to hold the final content response mapped to RefresherGuideContentResponse
        //        var contentResponseList = new List<RefresherGuideContentResponse>();

        //        foreach (var detail in contentList)
        //        {
        //            // Map the current chapter or content item
        //            var contentResponse = new RefresherGuideContentResponse
        //            {
        //                ContentId = detail.ContentId,
        //                SubjectId = detail.SubjectId,
        //                SyllabusId = detail.SyllabusID,
        //                IndexTypeId = detail.IndexTypeId,
        //                RegistrationId = request.RegistrationId,
        //                Synopsis = GetPDF(detail.Synopsis),
        //                ContentName = GetContentName(detail.IndexTypeId, detail.ContentId)
        //            };
        //            if (detail.IndexTypeId == 1)
        //            {
        //                contentResponseList.Add(contentResponse);
        //            }
        //        }

        //        // Return the service response with the mapped list
        //        if (contentResponseList != null && contentResponseList.Any())
        //        {
        //            return new ServiceResponse<List<RefresherGuideContentResponse>>(true, "Content retrieved successfully.", contentResponseList, 200);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<RefresherGuideContentResponse>>(false, "No content found for the given syllabus and subject.", null, 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<RefresherGuideContentResponse>>(false, ex.Message, null, 500);
        //    }
        //}
        private string GetContentName(int indexTypeId, int contentId)
        {
            string name = string.Empty;
            if (indexTypeId == 1)
            {
                // Query to fetch topics under the chapter
                var chapterQuery = @"
                SELECT  
                    ContentName_Chapter AS ContentName  
                FROM tblContentIndexChapters 
                WHERE ContentIndexId = @ContentIndexId AND IsActive = 1";

                // Fetch the topics
                name = _connection.QueryFirstOrDefault<string>(chapterQuery, new { ContentIndexId = contentId });
            }
            if (indexTypeId == 2)
            {
                // Query to fetch topics under the chapter
                var chapterQuery = @"
                SELECT  
                    ContentName_Topic AS ContentName  
                FROM tblContentIndexTopics 
                WHERE ContInIdTopic = @ContentIndexId AND IsActive = 1";

                // Fetch the topics
                name = _connection.QueryFirstOrDefault<string>(chapterQuery, new { ContentIndexId = contentId });
            }
            if (indexTypeId == 3)
            {
                // Query to fetch topics under the chapter
                var chapterQuery = @"
                SELECT  
                    ContentName_SubTopic AS ContentName  
                FROM tblContentIndexSubTopics 
                WHERE ContInIdSubTopic = @ContentIndexId AND IsActive = 1";

                // Fetch the topics
                name = _connection.QueryFirstOrDefault<string>(chapterQuery, new { ContentIndexId = contentId });
            }
            return name;
        }
        public async Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request)
        {
            try
            {

                var data = await _connection.QueryFirstOrDefaultAsync(@"select * from tblStudentClassCourseMapping where RegistrationID = 
                @RegistrationID", new { RegistrationID = request.RegistrationId });
                // Step 1: Base query to fetch questions
                string query = @"
        SELECT 
            q.QuestionId,
            q.QuestionCode,
            q.QuestionDescription,
            q.QuestionFormula,
            q.QuestionImage,
            q.DifficultyLevelId,
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
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        WHERE q.SubjectId = @SubjectId
        AND q.IndexTypeId = @IndexTypeId
        AND q.ContentIndexId = @ContentIndexId
        AND q.IsActive = 1 
        AND q.IsLive = 1  
 AND EXISTS (
              SELECT 1 
              FROM tblQIDCourse qc 
              WHERE qc.QuestionCode = q.QuestionCode 
                AND qc.LevelId = 2 AND qc.CourseID = @CourseID
          )";

                // Step 2: Add filters dynamically
                if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                {
                    query += " AND q.QuestionTypeId IN @QuestionTypeId";
                }

                // Step 3: Initialize parameters
                var parameters = new DynamicParameters();
                parameters.Add("@SubjectId", request.SubjectId);
                parameters.Add("@IndexTypeId", request.IndexTypeId);
                parameters.Add("@ContentIndexId", request.ContentIndexId);
                parameters.Add("@CourseID", data.CourseID);

                if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                {
                    parameters.Add("@QuestionTypeId", request.QuestionTypeId.ToArray());
                }

                // Fetch base questions
                var questions = (await _connection.QueryAsync<QuestionResponse>(query, parameters)).ToList();

                // Step 4: Check question status and filter results
                var filteredQuestions = new List<QuestionResponse>();

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
                                "SELECT COUNT(1) FROM tblRefresherGuideQuestionSave WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
                                new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
                            if (isSaved)
                            {
                                isPresentInAnyTable = true;
                            }
                        }

                        // Check if the question is in 'Read' table
                        if (request.QuestionStatus.Contains(17)) // Mark as Read
                        {
                            var isRead = await _connection.ExecuteScalarAsync<bool>(
                                "SELECT COUNT(1) FROM tblRefresherGuideQuestionRead WHERE QuestionID = @QuestionId AND StudentID = @StudentId",
                                new { QuestionId = question.QuestionId, StudentId = request.RegistrationId });
                            if (isRead)
                            {
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
            
                // Step 5: Fetch answers and map to questions
                foreach (var question in filteredQuestions)
                {
                    question.Answers = GetAnswersByQuestionCode(question.QuestionCode, question.QuestionId);
                    question.QIDCourseResponses = GetListOfQIDCourse(question.QuestionCode);
                }

                // Step 6: Apply pagination
                var paginatedResults = filteredQuestions
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Step 7: Return response
                if (paginatedResults.Any())
                {
                    return new ServiceResponse<List<QuestionResponse>>(true, "Questions retrieved successfully.", paginatedResults, 200);
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponse>>(false, "No questions found for the given criteria.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId)
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
            INSERT INTO tblRefresherGuideSharedQuestions (QuestionId, SharedBy, SharedTo)
            VALUES (@QuestionId, @SharedBy, @SharedTo)";

                int totalInserted = 0;
                foreach (var classmateId in classmates)
                {
                    int rows = await _connection.ExecuteAsync(insertQuery, new
                    {
                       // TestSeriesId = TestSeriesId,
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
        //public async Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request)
        //{
        //    try
        //    {
        //        // Base query for questions
        //        string query = @"
        //SELECT 
        //    q.QuestionId,
        //    q.QuestionCode,
        //    q.QuestionDescription,
        //    q.QuestionFormula,
        //    q.QuestionImage,
        //    q.DifficultyLevelId,
        //    q.QuestionTypeId,
        //    q.IndexTypeId,
        //    q.Status,
        //    q.Explanation,
        //    q.IsActive,
        //    q.IsLive,
        //    q.ExtraInformation,
        //    q.IsConfigure,
        //    q.CategoryId,
        //    qt.QuestionType AS QuestionType
        //FROM tblQuestion q
        //LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        //WHERE q.SubjectId = @SubjectId
        //AND q.IndexTypeId = @IndexTypeId
        //AND q.ContentIndexId = @ContentIndexId
        //AND q.IsActive = 1 
        //AND q.IsLive = 1";

        //        // Add the QuestionTypeId filter dynamically if it's not empty or null
        //        if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
        //        {
        //            query += " AND q.QuestionTypeId IN @QuestionTypeId";
        //        }

        //        // Initialize parameters
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@SubjectId", request.SubjectId);
        //        parameters.Add("@IndexTypeId", request.IndexTypeId);
        //        parameters.Add("@ContentIndexId", request.ContentIndexId);

        //        if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
        //        {
        //            parameters.Add("@QuestionTypeId", request.QuestionTypeId.ToArray());
        //        }

        //        // Fetch question data
        //        var baseResults = await _connection.QueryAsync(query, parameters);

        //        // Filter by QuestionStatus if provided
        //        List<int> filteredQuestionIds = new();
        //        if (request.QuestionStatus != null && request.QuestionStatus.Any())
        //        {
        //            foreach (var statusId in request.QuestionStatus)
        //            {
        //                if (statusId == 16) // Save
        //                {
        //                    var savedQuestions = await _connection.QueryAsync<int>(
        //                        "SELECT QuestionID FROM tblRefresherGuideQuestionSave WHERE StudentID = @StudentId",
        //                        new { StudentId = request.SubjectId }); // Replace StudentId with actual parameter if needed
        //                    filteredQuestionIds.AddRange(savedQuestions);
        //                }
        //                else if (statusId == 17) // Mark as Read
        //                {
        //                    var readQuestions = await _connection.QueryAsync<int>(
        //                        "SELECT QuestionID FROM tblRefresherGuideQuestionRead WHERE StudentID = @StudentId",
        //                        new { StudentId = request.SubjectId });
        //                    filteredQuestionIds.AddRange(readQuestions);
        //                }
        //                else if (statusId == 18) // Report
        //                {
        //                    var reportedQuestions = await _connection.QueryAsync<int>(
        //                        "SELECT QuestionID FROM tblReportedQuestions WHERE subjectID = @SubjectId",
        //                        new { SubjectId = request.SubjectId });
        //                    filteredQuestionIds.AddRange(reportedQuestions);
        //                }
        //            }

        //            // Filter base results by QuestionStatus IDs
        //            baseResults = baseResults.Where(q => filteredQuestionIds.Contains(q.QuestionId));
        //        }

        //        // Map results to QuestionResponse
        //        var mappedResults = baseResults.Select(row => new QuestionResponse
        //        {
        //            QuestionId = row.QuestionId,
        //            QuestionCode = row.QuestionCode,
        //            QuestionDescription = row.QuestionDescription,
        //            QuestionFormula = row.QuestionFormula,
        //            QuestionImage = row.QuestionImage,
        //            DifficultyLevelId = row.DifficultyLevelId,
        //            QuestionTypeId = row.QuestionTypeId,
        //            IndexTypeId = row.IndexTypeId,
        //            Explanation = row.Explanation,
        //            IsActive = row.IsActive,
        //            IsLive = row.IsLive,
        //            ExtraInformation = row.ExtraInformation,
        //            QuestionType = row.QuestionType,
        //            Answers = GetAnswersByQuestionCode(row.QuestionCode, row.QuestionId),
        //            QIDCourseResponses = GetListOfQIDCourse(row.QuestionCode)
        //        }).ToList();

        //        // Apply pagination
        //        var response = mappedResults
        //            .Skip((request.PageNumber - 1) * request.PageSize)
        //            .Take(request.PageSize)
        //            .ToList();

        //        // Return the response
        //        if (response.Any())
        //        {
        //            return new ServiceResponse<List<QuestionResponse>>(true, "Questions retrieved successfully.", response, 200);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<QuestionResponse>>(false, "No questions found for the given criteria.", null, 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<QuestionResponse>>(false, ex.Message, null, 500);
        //    }
        //}
        private AnswerResponse GetAnswersByQuestionCode(string questionCode, int Questionid)
        {
            string query = @"
    SELECT 
        am.AnswerId,
        am.QuestionCode,
        sac.Answer AS Answer
    FROM tblAnswerMaster am
    LEFT JOIN tblAnswerSingleAnswerCategory sac ON am.AnswerId = sac.AnswerId
    WHERE am.QuestionCode = @QuestionCode and am.Questionid = @Questionid";

            return  _connection.QueryFirstOrDefault<AnswerResponse>(query, new { QuestionCode = questionCode , Questionid = Questionid });
        }
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetDistinctQuestionTypes(int subjectId)
        {
            try
            {
                // SQL query to fetch distinct QuestionTypeId and their names for the given SubjectId
                string query = @"
        SELECT DISTINCT q.QuestionTypeId, qt.QuestionType
        FROM tblQuestion q
        INNER JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeId
        WHERE q.SubjectId = @SubjectId
        AND q.IsActive = 1";

                // Fetch distinct QuestionTypeIds and their names
                var questionTypes = await _connection.QueryAsync<QuestionTypeResponse>(
                    query,
                    new { SubjectId = subjectId }
                );

                var distinctQuestionTypes = questionTypes.ToList();

                if (distinctQuestionTypes.Any())
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(true, "Distinct Question Types retrieved successfully.", distinctQuestionTypes, 200);
                }
                else
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(false, "No distinct Question Types found for the given SubjectId.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRefresherGuidwRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);

            try
            {
                // Check if the question is already saved by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT RFQSID 
              FROM tblRefresherGuideQuestionSave 
              WHERE StudentID = @StudentId AND QuestionID = @QuestionId",
                      new { StudentId = request.RegistrationId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it
                    var deleteQuery = @"DELETE FROM tblRefresherGuideQuestionSave 
                                WHERE StudentID = @RegistrationId AND QuestionID = @QuestionId";
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
                    var insertQuery = @"INSERT INTO tblRefresherGuideQuestionSave (StudentID, QuestionID, QuestionCode) 
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
        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRefresherGuidwRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);

            try
            {
                // Check if the question is already marked as read by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT RGQRID 
              FROM tblRefresherGuideQuestionRead 
              WHERE StudentID = @RegistrationId AND QuestionID = @QuestionId",
                      new { request.RegistrationId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it (unmark as read)
                    var deleteQuery = @"DELETE FROM tblRefresherGuideQuestionRead 
                                WHERE StudentID = @RegistrationId AND QuestionID = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId
                    });

                    if (rowsDeleted > 0)
                    {
                        response.Data = "Question unread successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to unread the question.";
                        response.Success = false;
                    }
                }
                else
                {
                    // If no record exists, insert a new read question record
                    var insertQuery = @"INSERT INTO tblRefresherGuideQuestionRead (StudentID, QuestionID, QuestionCode) 
                                VALUES (@RegistrationId, @QuestionId, @QuestionCode)";

                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    if (rowsInserted > 0)
                    {
                        response.Data = "Question marked as read successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to mark the question as read.";
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
        public async Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            List<RefresherGuideContentResponse> contentResponse = new List<RefresherGuideContentResponse>();

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

                    contentResponse = (await _connection.QueryAsync<RefresherGuideContentResponse>(queryChapters, new
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

                    contentResponse = (await _connection.QueryAsync<RefresherGuideContentResponse>(queryTopics, new
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

                    contentResponse = (await _connection.QueryAsync<RefresherGuideContentResponse>(querySubTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                    })).ToList();
                }
                foreach (var data in contentResponse)
                {
                    data.RegistrationId = request.RegistrationId;
                    data.Percentage = PercentageCalculation(data.IndexTypeId, data.ContentId, request.RegistrationId, request.SubjectId, request.SyllabusId);
                }
                return new ServiceResponse<List<RefresherGuideContentResponse>>(true, "Success", contentResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RefresherGuideContentResponse>>(false, ex.Message, null, 500);
            }
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
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Syllabus", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private List<QIDCourseResponse> GetListOfQIDCourse(string QuestionCode)
        {
            // Get active question IDs
            var activeQuestionIds = GetActiveQuestionIds(QuestionCode);

            // If no active question IDs found, return an empty list
            if (!activeQuestionIds.Any())
            {
                return new List<QIDCourseResponse>();
            }

            var query = @"
    SELECT qc.*, c.CourseName, l.LevelName
    FROM [tblQIDCourse] qc
    LEFT JOIN tblCourse c ON qc.CourseID = c.CourseID
    LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
    WHERE qc.QuestionCode = @QuestionCode
      AND qc.QID IN @ActiveQuestionIds";

            var data = _connection.Query<QIDCourseResponse>(query, new { QuestionCode, ActiveQuestionIds = activeQuestionIds });
            return data.ToList();
        }
        private List<int> GetActiveQuestionIds(string QuestionCode)
        {
          //  if (_connection.Open) { }then close
            var query = @"
            SELECT q.QuestionId
            FROM tblQuestion q
            WHERE q.QuestionCode = @QuestionCode
              AND q.IsActive = 1 AND q.IsConfigure = 1";
            
           var questionIds = _connection.Query<int>(query, new { QuestionCode });
            return questionIds.ToList();
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
