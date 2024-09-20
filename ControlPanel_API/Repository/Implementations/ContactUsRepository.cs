using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
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
                string sql = @"
        SELECT 
            cu.ContactusID, 
            cu.Querytype AS Querytype,
            q.QueryType AS QueryTypeName,
            cu.QuerytypeDescription AS QuerytypeDescription,
            cu.EmployeeID AS EmployeeID,
            e.EmpFirstName AS EmpFirstName,
            cu.phonenumber AS phonenumber,
            e.EMPEmail AS Email,
            cu.boardid AS boardid,
            b.BoardName AS Board,
            cu.classid AS classid,
            c.ClassName AS Class,
            cu.courseid AS courseid,
            cr.CourseName AS Course,
            cu.APID AS APID,
            ct.APName AS Category,
            cu.username AS username,
            cu.DateTime AS DateTime,
            cu.RQSID AS RQSID,
            s.RQSName AS RQSName,
            cu.ExamTypeId as ExamTypeId,
            ex.[ExamTypeName] as ExamTypeName
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
            [dbo].[tblEmployee] e ON e.Employeeid = cu.EmployeeID
        LEFT JOIN 
            [dbo].[tblStatus] s ON s.RQSID = cu.RQSID
        LEFT JOIN 
            [dbo].[tblExamType] ex ON ex.[ExamTypeID] = cu.ExamTypeId
        WHERE 
            1 = 1";

                var parameters = new DynamicParameters();

                // Add filters based on DTO properties
                if (request.BoardID.HasValue && request.BoardID > 0)
                {
                    sql += " AND cu.BoardId = @BoardID";
                    parameters.Add("BoardID", request.BoardID);
                }
                if (request.CourseId.HasValue && request.CourseId > 0)
                {
                    sql += " AND cu.CourseId = @CourseId";
                    parameters.Add("CourseId", request.CourseId);
                }
                if (request.ClassId.HasValue && request.ClassId > 0)
                {
                    sql += " AND cu.ClassId = @ClassId";
                    parameters.Add("ClassId", request.ClassId);
                }
                if (request.APID > 0)
                {
                    sql += " AND cu.APID = @APID";
                    parameters.Add("APID", request.APID);
                }
                if (request.ExamTypeId > 0)
                {
                    sql += " AND cu.ExamTypeId = @ExamTypeId";
                    parameters.Add("ExamTypeId", request.ExamTypeId);
                }
                if (request.StartDate.HasValue)
                {
                    sql += " AND cu.[DateTime] >= @StartDate";
                    parameters.Add("StartDate", request.StartDate);
                }
                if (request.EndDate.HasValue)
                {
                    sql += " AND cu.[DateTime] <= @EndDate";
                    parameters.Add("EndDate", request.EndDate);
                }
                if (request.Today.HasValue)
                {
                    sql += " AND cu.[DateTime] = @Today";
                    parameters.Add("Today", request.Today);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    sql += " AND cu.[ContactusID] LIKE @SearchText";
                    parameters.Add("SearchText", "%" + request.SearchText + "%");
                }

                var list = await _connection.QueryAsync<GetAllContactUsResponse>(sql, parameters);
                var paginatedList = list.Skip((request.PageNumber - 1) * request.PageSize)
                             .Take(request.PageSize)
                             .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<GetAllContactUsResponse>>(true, "Records Found", paginatedList, 200, list.Count());
                }
                else
                {
                    return new ServiceResponse<List<GetAllContactUsResponse>>(false, "Records Not Found", new List<GetAllContactUsResponse>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllContactUsResponse>>(false, ex.Message, new List<GetAllContactUsResponse>(), 500);
            }
        }
        public async Task<ServiceResponse<GetAllContactUsResponse>> GetContactUsById(int contactusId)
        {
            try
            {
                string sql = @"
        SELECT 
            cu.ContactusID, 
            cu.Querytype AS Querytype,
            q.QueryType AS QueryTypeName,
            cu.QuerytypeDescription AS QuerytypeDescription,
            cu.EmployeeID AS EmployeeID,
            e.EmpFirstName AS EmpFirstName,
            cu.phonenumber AS phonenumber,
            e.EMPEmail AS Email,
            cu.boardid AS boardid,
            b.BoardName AS Board,
            cu.classid AS classid,
            c.ClassName AS Class,
            cu.courseid AS courseid,
            cr.CourseName AS Course,
            cu.APID AS APID,
            ct.APName AS Category,
            cu.username AS username,
            cu.DateTime AS DateTime,
            cu.RQSID AS RQSID,
            s.RQSName AS RQSName,
            cu.ExamTypeId as ExamTypeId,
            ex.[ExamTypeName] as ExamTypeName
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
            [dbo].[tblEmployee] e ON e.Employeeid = cu.EmployeeID
        LEFT JOIN 
            [dbo].[tblStatus] s ON s.RQSID = cu.RQSID
        LEFT JOIN 
            [dbo].[tblExamType] ex ON ex.[ExamTypeID] = cu.ExamTypeId
        WHERE 
            cu.ContactusID = @ContactusID";

                var parameters = new { ContactusID = contactusId };

                var contact = await _connection.QueryFirstOrDefaultAsync<GetAllContactUsResponse>(sql, parameters);

                if (contact != null)
                {
                    string updateQuery = @"
            UPDATE [tblHelpContactUs]
            SET RQSID = @StatusId, modifiedon = @ModifiedOn, modifiedby = @ModifiedBy
            WHERE ContactusID = @ContactusId";

                    await _connection.ExecuteAsync(updateQuery, new
                    {
                        StatusId = 1,
                        ModifiedOn = DateTime.UtcNow,
                        ModifiedBy = contact.EmpFirstName, // Replace with the actual user ID or username
                        contactusId
                    });
                    return new ServiceResponse<GetAllContactUsResponse>(true, "Record Found", contact, 200);
                }
                else
                {
                    return new ServiceResponse<GetAllContactUsResponse>(false, "Contact not found", new GetAllContactUsResponse(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GetAllContactUsResponse>(false, ex.Message, new GetAllContactUsResponse(), 500);
            }
        }
    }
}
