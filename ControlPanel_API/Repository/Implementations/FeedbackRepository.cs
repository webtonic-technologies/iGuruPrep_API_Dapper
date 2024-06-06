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

        public async Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request)
        {
            try
            {
                string sql = @"
    SELECT 
        fb.FeedBackId, 
        fb.FirstName AS Name,
        fb.FeedbackDescription AS FeedBackDesc,
        fb.Rating AS Rating,
        fb.Date AS Date,
        fb.phonenumber AS PhoneNumber,
        fb.Email AS Email,
        b.BoardName AS Board,
        c.ClassName AS Class,
        cr.CourseName AS Course
    FROM 
        [tblUserFeedBack] fb
    LEFT JOIN 
        [dbo].[tblBoard] b ON b.BoardId = fb.BoardId
    LEFT JOIN 
        [dbo].[tblClass] c ON c.ClassId = fb.ClassId
    LEFT JOIN 
        [dbo].[tblCourse] cr ON cr.CourseId = fb.CourseId
    WHERE 
        (fb.BoardId = @BoardId OR @BoardId = 0)
        AND (fb.CourseId = @CourseId OR @CourseId = 0)
        AND (fb.ClassId = @ClassId OR @ClassId = 0)
        AND (fb.APID = @APID OR @APID = 0)
        AND (@StartDate IS NULL OR fb.Date >= @StartDate)
        AND (@EndDate IS NULL OR fb.Date <= @EndDate)
        AND (@Today IS NULL OR fb.Date = @Today);";

                var list = await _connection.QueryAsync<GetAllFeedbackResponse>(sql, new { request.BoardID, request.ClassId, request.CourseId, request.APID, request.StartDate
                ,request.EndDate, request.Today
                });
                var paginatedList = list.Skip((request.PageNumber - 1) * request.PageSize)
                              .Take(request.PageSize)
                              .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(true, "Records Found", paginatedList.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllFeedbackResponse>>(false, ex.Message, [], 500);
            }
        }
    }
}
