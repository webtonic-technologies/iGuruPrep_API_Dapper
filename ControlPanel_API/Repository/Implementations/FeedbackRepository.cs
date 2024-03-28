using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly IDbConnection _connection;

        public FeedbackRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddFeedBack(Feedback request)
        {
            try
            {
                string sql = @"INSERT INTO tblUserFeedback (CreatedDate, FeedbackDesc, FeedbackTypeID, ParentfeedbackID, Rating, Status, SyllabusID, UserID) 
                       VALUES (@CreatedDate, @FeedbackDesc, @FeedbackTypeID, @ParentfeedbackID, @Rating, @Status, @SyllabusID, @UserID)";

                int rowsAffected = await _connection.ExecuteAsync(sql, request);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Feedback Added Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateFeedback(Feedback request)
        {
            try
            {
                string sql = @"UPDATE tblUserFeedback 
                       SET CreatedDate = @CreatedDate, 
                           FeedbackDesc = @FeedbackDesc, 
                           FeedbackTypeID = @FeedbackTypeID, 
                           ParentfeedbackID = @ParentfeedbackID, 
                           Rating = @Rating, 
                           Status = @Status, 
                           SyllabusID = @SyllabusID, 
                           UserID = @UserID 
                       WHERE FeedbackID = @FeedbackID";

                int rowsAffected = await _connection.ExecuteAsync(sql, new
                {
                    request.CreatedDate,
                    request.FeedbackDesc,
                    request.FeedbackTypeID,
                    request.ParentfeedbackID,
                    request.Rating,
                    request.Status,
                    request.SyllabusID,
                    request.UserID,
                    request.FeedbackID 
                });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Feedback Updated Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "Record not found", 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddSyllabus(Syllabus request)
        {
            try
            {
                // Construct the SQL insert query
                string sql = @"INSERT INTO tblSyllabus (CreatedOn, BoardID, ClassId, CourseId, CreatedBy, Description, ModifiedBy, ModifiedOn, Status, SyllabusName, YearID) 
                       VALUES (@CreatedOn, @BoardID, @ClassId, @CourseId, @CreatedBy, @Description, @ModifiedBy, @ModifiedOn, @Status, @SyllabusName, @YearID)";


                int rowsAffected = await _connection.ExecuteAsync(sql, new
                {
                    request.CreatedOn,
                    request.BoardID,
                    request.ClassId,
                    request.CourseId,
                    request.CreatedBy,
                    request.Description,
                    request.ModifiedBy,
                    request.ModifiedOn,
                    //request.SyllabusId,
                    request.Status,
                    request.SyllabusName,
                    request.YearID
                });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Added Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request)
        {
            try
            {
                var list = new List<GetAllFeedbackResponse>();

                string sql = @"SELECT * FROM tblSyllabus WHERE (BoardID = @BoardID OR @BoardID = 0)
                            AND (CourseId = @CourseId OR @CourseId = 0)
                            AND (ClassId = @ClassId OR @ClassId = 0)";

                // Execute the SQL query and retrieve syllabi asynchronously using Dapper
                IEnumerable<Syllabus> syllabi = await _connection.QueryAsync<Syllabus>(sql, new
                {
                    request.BoardID,
                    request.ClassId,
                    request.CourseId
                });

                foreach (var syllabus in syllabi)
                {
                    string feedbackSql = "SELECT * FROM tblUserFeedback WHERE SyllabusID = @SyllabusID";

                    var feedback = _connection.QueryFirstOrDefault<Feedback>(feedbackSql, new { SyllabusID = syllabus.SyllabusId });

                    if (feedback != null)
                    {
                        // Map the retrieved feedback and syllabus details to the desired response object
                        var feedbackResponse = new GetAllFeedbackResponse
                        {
                            Rating = feedback.Rating,
                            Date = feedback.CreatedDate,
                            FeedBackDesc = feedback.FeedbackDesc,
                            Board = GetBoardNameById(syllabus.BoardID),
                            Class = GetClassNameById(syllabus.ClassId),
                            Course = GetCourseNameById(syllabus.CourseId),
                        };
                        list.Add(feedbackResponse);
                    }
                }


                if (list != null)
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(true, "Records Found", list, 200);
                }
                else
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(false, "Records Not Found", new List<GetAllFeedbackResponse>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllFeedbackResponse>>(false, ex.Message, new List<GetAllFeedbackResponse>(), 500);
            }
        }

        private string GetBoardNameById(int? boardId)
        {
            string sql = "SELECT BoardName FROM tblBoard WHERE BoardId = @BoardId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { BoardId = boardId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        // Helper method to get class name by ID
        private string GetClassNameById(int? classId)
        {
            string sql = "SELECT ClassName FROM tblClass WHERE ClassId = @ClassId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { ClassId = classId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        // Helper method to get course name by ID
        private string GetCourseNameById(int? courseId)
        {
            string sql = "SELECT CourseName FROM tblCourse WHERE CourseId = @CourseId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { CourseId = courseId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
