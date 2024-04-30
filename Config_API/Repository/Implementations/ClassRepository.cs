using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class ClassRepository : IClassRepository
    {

        private readonly IDbConnection _connection;

        public ClassRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateClass(Class request)
        {
            try
            {
                if (request.ClassId == 0)
                {
                    var newClass = new Class
                    {
                        ClassCode = request.ClassCode,
                        ClassName = request.ClassName,
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        EmployeeID = request.EmployeeID,
                        showcourse = request.showcourse,
                        Status = true
                    };
                    string insertQuery = @"INSERT INTO [iGuruPrep].[dbo].[tblClass] 
                           ([ClassName], [ClassCode], [Status], [createdby], [createdon], [showcourse], [EmployeeID])
                           VALUES (@ClassName, @ClassCode, @Status, @CreatedBy, @CreatedOn, @ShowCourse, @EmployeeID)";
                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newClass);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE [iGuruPrep].[dbo].[tblClass] 
                           SET [ClassName] = @ClassName, [ClassCode] = @ClassCode, [Status] = @Status, 
                               [modifiedby] = @ModifiedBy, [modifiedon] = @ModifiedOn, [showcourse] = @ShowCourse, [EmployeeID] = @EmployeeID
                           WHERE [ClassId] = @ClassId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.ClassName,
                        request.ClassCode,
                        request.Status,
                        request.EmployeeID,
                        request.showcourse,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.ClassId
                    });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<List<Class>>> GetAllClasses()
        {
            try
            {
                string query = @"SELECT [ClassId]
                                  ,[ClassName]
                                  ,[ClassCode]
                                  ,[Status]
                                  ,[createdby]
                                  ,[createdon]
                                  ,[modifiedby]
                                  ,[modifiedon]
                                  ,[showcourse]
                                  ,[EmployeeID]
                           FROM [iGuruPrep].[dbo].[tblClass]";
                var classes = await _connection.QueryAsync<Class>(query);

                if (classes != null)
                {
                    return new ServiceResponse<List<Class>>(true, "Records Found", classes.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Class>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<Class>> GetClassById(int id)
        {
            try
            {
                string query = @"SELECT [ClassId]
                                  ,[ClassName]
                                  ,[ClassCode]
                                  ,[Status]
                                  ,[createdby]
                                  ,[createdon]
                                  ,[modifiedby]
                                  ,[modifiedon]
                                  ,[showcourse]
                                  ,[EmployeeID]
                           FROM [iGuruPrep].[dbo].[tblClass]
                           WHERE [ClassId] = @ClassId";

                var classObj = await _connection.QueryFirstOrDefaultAsync<Class>(query, new { ClassId = id });

                if (classObj != null)
                {
                    return new ServiceResponse<Class>(true, "Record Found", classObj, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Class>(false, "Record not Found", new Class(), StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Class>(false, ex.Message, new Class(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                string query = @"
                SELECT ClassId, Status
                FROM tblClass
                WHERE ClassId = @Id";

                var classObj = await _connection.QueryFirstOrDefaultAsync<Class>(query, new { Id = id });

                if (classObj != null)
                {
                    // Toggle the status
                    classObj.Status = !classObj.Status;

                    string updateQuery = @"
                    UPDATE tblClass
                    SET Status = @Status
                    WHERE ClassId = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { classObj.Status, Id = id });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, StatusCodes.Status304NotModified);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, StatusCodes.Status500InternalServerError);
            }
        }
    }
}
