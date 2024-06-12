using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
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
        public async Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblUserFeedBack]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
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

                var list = await _connection.QueryAsync<GetAllFeedbackResponse>(sql, new
                {
                    request.BoardID,
                    request.ClassId,
                    request.CourseId,
                    request.APID,
                    request.StartDate,
                    request.EndDate,
                    request.Today
                });
                var paginatedList = list.Skip((request.PageNumber - 1) * request.PageSize)
                              .Take(request.PageSize)
                              .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(true, "Records Found", paginatedList.AsList(), 200, totalCount);
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
