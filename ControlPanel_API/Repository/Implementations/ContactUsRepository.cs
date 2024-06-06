using ControlPanel_API.DTOs;
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
        public async Task<ServiceResponse<string>> AddTicket(ContactUs request)
        {
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(
                            @"INSERT INTO tblTicket (TicketID, boardid, ClassId, Boardname, ClassName, CourseName, DateAndTime, MobileNumber, QueryInfo, QueryType, Status, SubjectName, TicketNo) 
              VALUES (@TicketID, @boardid, @ClassId, @Boardname, @ClassName, @CourseName, @DateAndTime, @MobileNumber, @QueryInfo, @QueryType, @Status, @SubjectName, @TicketNo)",
                            request);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Ticket Added Successfully", 200);
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

        public async Task<ServiceResponse<List<GetAllContactUsResponse>>> GetAllContactUs(GeAllContactUsRequest request)
        {
            try
            {
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
        AND (@Today IS NULL OR cu.[DateTime] = @Today);";

                var list = await _connection.QueryAsync<GetAllContactUsResponse>(sql, new
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
                    return new ServiceResponse<List<GetAllContactUsResponse>>(true, "Records Found", paginatedList.AsList(), 200);
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
