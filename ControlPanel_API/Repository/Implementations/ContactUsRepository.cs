using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class ContactUsRepository : IContactUsRepository
    {
        private readonly IDbConnection _connection;

        public ContactUsRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<GetAllContactUsResponse>>> GetAllContactUs(GeAllContactUsRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblHelpContactUs]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                string sql = @"
    SELECT 
        cu.ContactusID, 
        cu.QuerytypeDescription AS QuerytypeDescription,
        cu.phonenumber AS phonenumber,
        cu.username AS username,
        cu.DateTime AS DateTime,
        e.EmpFirstName AS EmpFirstName,
        b.BoardName AS Board,
        c.ClassName AS Class,
        cr.CourseName AS Course,
        ct.APName AS Category,
        q.[QueryType] AS QueryTypeName,
        e.EMPEmail AS Email

    FROM 
        [tblHelpContactUs] cu
    LEFT JOIN 
        [dbo].[tblBoard] b ON b.BoardId = cu.BoardId
    LEFT JOIN 
        [dbo].[tblClass] c ON c.ClassId = cu.ClassId
    LEFT JOIN 
        [dbo].[tblCourse] cr ON cr.CourseId = cu.CourseId
    LEFT JOIN 
        [dbo].[tblCategory] ct ON ct.APId = cu.APId
  LEFT JOIN 
        [dbo].[tblQuery] q ON q.QueryID = cu.QueryType
  LEFT JOIN 
        [dbo].[tblEmployee] e ON e.Employeeid = cu.[EmployeeID ]
    WHERE 
        (cu.BoardId = @BoardId OR @BoardId = 0)
        AND (cu.CourseId = @CourseId OR @CourseId = 0)
        AND (cu.ClassId = @ClassId OR @ClassId = 0)
        AND (cu.APID = @APID OR @APID = 0)
        AND (@StartDate IS NULL OR cu.[DateTime] >= @StartDate)
        AND (@EndDate IS NULL OR cu.[DateTime] <= @EndDate)
        AND (@Today IS NULL OR cu.[DateTime] = @Today)
        AND (@SearchText IS NULL OR [ContactusID] LIKE @SearchText);";

                var list = await _connection.QueryAsync<GetAllContactUsResponse>(sql, new
                {
                    request.BoardID,
                    request.ClassId,
                    request.CourseId,
                    request.APID,
                    request.StartDate,
                    request.EndDate,
                    request.Today,
                    request.SearchText
                });
                var paginatedList = list.Skip((request.PageNumber - 1) * request.PageSize)
                           .Take(request.PageSize)
                           .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<GetAllContactUsResponse>>(true, "Records Found", paginatedList.AsList(), 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<GetAllContactUsResponse>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllContactUsResponse>>(false, ex.Message, [], 500);
            }
        }
    }
}
