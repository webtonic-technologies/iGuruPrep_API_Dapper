using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class RefresherGuideRepository: IRefresherGuideRepository
    {
        private readonly IDbConnection _connection;

        public RefresherGuideRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<RefresherGuideSubjectsResposne>>> GetSyllabusSubjects(RefresherGuideRequest request)
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
                    SyllabusId = m.SyllabusId
                }).ToList();
                // Check if any subjects were found
                if (subjects != null && subjects.Count != 0)
                {
                    return new ServiceResponse<List<RefresherGuideSubjectsResposne>>(true, "Subjects retrieved successfully.", subjects.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<RefresherGuideSubjectsResposne>>(false, "No subjects found for the given criteria.", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RefresherGuideSubjectsResposne>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContent(GetContentRequest request)
        {
            try
            {
                // SQL query to fetch ContentId, SubjectId, IndexTypeId, and SyllabusID from SyllabusDetails
                string query = @"
        SELECT 
            sd.ContentIndexId AS ContentId,
            sd.SubjectId,
            sd.IndexTypeId,
            sd.SyllabusID
        FROM tblSyllabusDetails sd
        WHERE sd.SyllabusID = @SyllabusId AND sd.SubjectId = @SubjectId";

                // Fetch syllabus content details
                var contentList = await _connection.QueryAsync<dynamic>(query, new { request.SyllabusId, request.SubjectId });

                // List to hold the final content response mapped to RefresherGuideContentResponse
                var contentResponseList = new List<RefresherGuideContentResponse>();

                foreach (var detail in contentList)
                {
                    // Map the current chapter or content item
                    var contentResponse = new RefresherGuideContentResponse
                    {
                        ContentId = detail.ContentId,
                        SubjectId = detail.SubjectId,
                        SyllabusId = detail.SyllabusID,
                        IndexTypeId = detail.IndexTypeId,
                        ContentName = GetContentName(detail.IndexTypeId, detail.ContentId)
                    };
                    contentResponseList.Add(contentResponse);
                }

                // Return the service response with the mapped list
                if (contentResponseList != null && contentResponseList.Any())
                {
                    return new ServiceResponse<List<RefresherGuideContentResponse>>(true, "Content retrieved successfully.", contentResponseList, 200);
                }
                else
                {
                    return new ServiceResponse<List<RefresherGuideContentResponse>>(false, "No content found for the given syllabus and subject.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RefresherGuideContentResponse>>(false, ex.Message, null, 500);
            }
        }
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
            AND q.QuestionTypeId IN (3, 7, 8)  -- SA, LA, VSA
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
                    param: new { request.SubjectId, request.IndexTypeId, request.ContentIndexId }
                );

                // Check if any questions were found
                if (questionList != null && questionList.Any())
                {
                    return new ServiceResponse<List<QuestionResponse>>(true, "Questions retrieved successfully.", questionList.ToList(), 200);
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
                      new { request.StudentId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it
                    var deleteQuery = @"DELETE FROM tblRefresherGuideQuestionSave 
                                WHERE StudentID = @StudentId AND QuestionID = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.StudentId,
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
                                VALUES (@StudentId, @QuestionId, @QuestionCode)";

                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
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
              WHERE StudentID = @StudentId AND QuestionID = @QuestionId",
                      new { request.StudentId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it (unmark as read)
                    var deleteQuery = @"DELETE FROM tblRefresherGuideQuestionRead 
                                WHERE StudentID = @StudentId AND QuestionID = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.StudentId,
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
                                VALUES (@StudentId, @QuestionId, @QuestionCode)";

                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
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
                        ContentIndexId = request.ContentIndexId
                    })).ToList();
                }

                return new ServiceResponse<List<RefresherGuideContentResponse>>(true, "Success", contentResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RefresherGuideContentResponse>>(false, ex.Message, null, 500);
            }
        }
    }
}
