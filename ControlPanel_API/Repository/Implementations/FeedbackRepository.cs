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
                    cr.CourseName AS Course,
                    ct.APName AS APName,
                    et.ExamTypeName AS ExamTypeName,
                    fb.BoardId,
                    fb.ClassId,
                    fb.CourseId,
                    fb.APID,
                    fb.ExamTypeId
                FROM 
                    [tblUserFeedBack] fb
                LEFT JOIN 
                    [dbo].[tblBoard] b ON b.BoardId = fb.BoardId
                LEFT JOIN 
                    [dbo].[tblClass] c ON c.ClassId = fb.ClassId
                LEFT JOIN 
                    [dbo].[tblCourse] cr ON cr.CourseId = fb.CourseId
                LEFT JOIN 
                    [dbo].[tblCategory] ct ON ct.APID = fb.APID
                LEFT JOIN 
                    [dbo].[tblExamType] et ON et.ExamTypeId = fb.ExamTypeId
                WHERE 
                    (fb.BoardId = @BoardId OR @BoardId = 0)
                    AND (fb.CourseId = @CourseId OR @CourseId = 0)
                    AND (fb.ClassId = @ClassId OR @ClassId = 0)
                    AND (fb.ExamTypeId = @ExamTypeId OR @ExamTypeId = 0)
                    AND (fb.APID = @APID OR @APID = 0)
                    AND (@StartDate IS NULL OR fb.Date >= @StartDate)
                    AND (@EndDate IS NULL OR fb.Date <= @EndDate)
                    AND (@Today IS NULL OR fb.Date = @Today);";

                var parameters = new
                {
                    request.BoardID,
                    request.ClassId,
                    request.CourseId,
                    request.APID,
                    request.StartDate,
                    request.EndDate,
                    request.Today
                };

                var list = await _connection.QueryAsync<GetAllFeedbackResponse>(sql, parameters);
                var paginatedList = list.Skip((request.PageNumber - 1) * request.PageSize)
                                        .Take(request.PageSize)
                                        .ToList();
                if (paginatedList.Any())
                {
                    return new ServiceResponse<List<GetAllFeedbackResponse>>(true, "Records Found", paginatedList, 200, list.Count());
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
        public async Task<ServiceResponse<GetAllFeedbackResponse>> GetFeedbackById(int feedbackId)
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
            cr.CourseName AS Course,
            ct.APName AS APName,
            et.ExamTypeName AS ExamTypeName,
            fb.BoardId,
            fb.ClassId,
            fb.CourseId,
            fb.APID,
            fb.ExamTypeId
        FROM 
            [tblUserFeedBack] fb
        LEFT JOIN 
            [dbo].[tblBoard] b ON b.BoardId = fb.BoardId
        LEFT JOIN 
            [dbo].[tblClass] c ON c.ClassId = fb.ClassId
        LEFT JOIN 
            [dbo].[tblCourse] cr ON cr.CourseId = fb.CourseId
        LEFT JOIN 
            [dbo].[tblCategory] ct ON ct.APID = fb.APID
        LEFT JOIN 
            [dbo].[tblExamType] et ON et.ExamTypeId = fb.ExamTypeId
        WHERE 
            fb.FeedBackId = @FeedbackId;";

                var feedback = await _connection.QuerySingleOrDefaultAsync<GetAllFeedbackResponse>(sql, new { FeedbackId = feedbackId });

                if (feedback != null)
                {
                    return new ServiceResponse<GetAllFeedbackResponse>(true, "Record Found", feedback, 200);
                }
                else
                {
                    return new ServiceResponse<GetAllFeedbackResponse>(false, "Record Not Found", new GetAllFeedbackResponse(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GetAllFeedbackResponse>(false, ex.Message, new GetAllFeedbackResponse(), 500);
            }
        }

    }
}
