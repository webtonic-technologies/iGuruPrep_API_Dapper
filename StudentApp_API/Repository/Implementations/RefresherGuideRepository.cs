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
                string query = @"
        SELECT 
            q.QuestionId,
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
            q.IsConfigure,
            q.CategoryId,
            q.QuestionCode,
            am.AnswerId,
            am.QuestionCode,
            sac.Answer AS Answer
        FROM tblQuestion q
        LEFT JOIN tblAnswerMaster am ON q.QuestionCode = am.QuestionCode
        LEFT JOIN tblAnswerSingleAnswerCategory sac ON am.AnswerId = sac.AnswerId
        WHERE q.SubjectId = @SubjectId
        AND q.IndexTypeId = @IndexTypeId
        AND q.ContentIndexId = @ContentIndexId
        AND q.IsActive = 1
        AND 
        (
            @QuestionTypeId IS NULL OR q.QuestionTypeId IN @QuestionTypeId
            OR (@QuestionTypeId IS NULL AND q.QuestionTypeId IN (3, 7, 8)) -- Default types
        )
        ORDER BY q.QuestionId";

                // Directly map the query results to the QuestionResponse and AnswerResponse objects
                var questionList = await _connection.QueryAsync<QuestionResponse, AnswerResponse, QuestionResponse>(
                    query,
                    (question, answer) =>
                    {
                        question.Answers = answer; // Map the answer directly to the question's Answers property
                        return question;
                    },
                    splitOn: "AnswerId", // Dapper will split at AnswerId to map the second object
                    param: new
                    {
                        request.SubjectId,
                        request.IndexTypeId,
                        request.ContentIndexId,
                        QuestionTypeId = request.QuestionTypeId?.Count > 0 ? request.QuestionTypeId : null
                    }
                );

                var response = questionList
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if any questions were found
                if (questionList != null && questionList.Any())
                {
                    return new ServiceResponse<List<QuestionResponse>>(true, "Questions retrieved successfully.", response, 200);
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
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetDistinctQuestionTypes(int subjectId)
        {
            try
            {
                // SQL query to fetch distinct QuestionTypeId and their names for the given SubjectId
                string query = @"
        SELECT DISTINCT q.QuestionTypeId, qt.QuestionTypeName
        FROM tblQuestion q
        INNER JOIN tblQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeId
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
        //public async Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request)
        //{
        //    try
        //    {
        //        string query = @"
        //    SELECT 
        //        q.QuestionId,
        //        q.QuestionDescription,
        //        q.QuestionFormula,
        //        q.QuestionImage,
        //        q.DifficultyLevelId,
        //        q.QuestionTypeId,
        //        q.IndexTypeId,
        //        q.Status,
        //        q.Explanation,
        //        q.IsActive,
        //        q.IsLive,
        //        q.IsConfigure,
        //        q.CategoryId,
        //        q.QuestionCode,
        //        am.AnswerId,
        //        am.QuestionCode,
        //        sac.Answer AS Answer
        //    FROM tblQuestion q
        //    LEFT JOIN tblAnswerMaster am ON q.QuestionCode = am.QuestionCode
        //    LEFT JOIN tblAnswerSingleAnswerCategory sac ON am.AnswerId = sac.AnswerId
        //    WHERE q.SubjectId = @SubjectId
        //    AND q.IndexTypeId = @IndexTypeId
        //    AND q.ContentIndexId = @ContentIndexId
        //    AND q.IsActive = 1
        //    AND q.QuestionTypeId IN (3, 7, 8)  -- SA, LA, VSA
        //    ORDER BY q.QuestionId";

        //        // Directly map the query results to the QuestionResponse and AnswerResponse objects
        //        var questionList = await _connection.QueryAsync<QuestionResponse, AnswerResponse, QuestionResponse>(
        //            query,
        //            (question, answer) =>
        //            {
        //                question.Answers = answer; // Map the answer directly to the question's Answers property
        //                return question;
        //            },
        //            splitOn: "AnswerId", // Dapper will split at AnswerId to map the second object
        //            param: new { request.SubjectId, request.IndexTypeId, request.ContentIndexId }
        //        );
        //        var response  = questionList
        //          .Skip((request.PageNumber - 1) * request.PageSize)
        //          .Take(request.PageSize)
        //          .ToList();
        //        // Check if any questions were found
        //        if (questionList != null && questionList.Any())
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
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);

            try
            {
                // Check if the question is already saved by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT RFQSID 
              FROM tblRefresherGuideQuestionSave 
              WHERE StudentID = @StudentId AND QuestionID = @QuestionId",
                      new { request.RegistrationId, request.QuestionId });

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
        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request)
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
     
    }
}
