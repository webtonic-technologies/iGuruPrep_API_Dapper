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
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.Now,
                        DisplayOrder = request.DisplayOrder,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = request.ModifiedOn,
                        ShowCourse = request.ShowCourse,
                        Status = request.Status,
                    };

                    string insertQuery = @"
                    INSERT INTO tblClass (ClassCode, ClassName, CreatedBy, CreatedOn, DisplayOrder, ModifiedBy, ModifiedOn, ShowCourse, Status)
                    VALUES (@ClassCode, @ClassName, @CreatedBy, @CreatedOn, @DisplayOrder, @ModifiedBy, @ModifiedOn, @ShowCourse, @Status)";
                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newClass);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
                    UPDATE tblClass
                    SET ClassCode = @ClassCode, ClassName = @ClassName, CreatedBy = @CreatedBy, CreatedOn = @CreatedOn,
                        DisplayOrder = @DisplayOrder, ModifiedBy = @ModifiedBy, ModifiedOn = @ModifiedOn,
                        ShowCourse = @ShowCourse, Status = @Status
                    WHERE ClassId = @ClassId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, request);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Class>>> GetAllClasses()
        {
            try
            {
                string query = @"SELECT * FROM tblClass";
                var classes = await _connection.QueryAsync<Class>(query);

                if (classes != null)
                {
                    return new ServiceResponse<List<Class>>(true, "Records Found", classes.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Class>>(false, "Records Not Found", new List<Class>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, new List<Class>(), 200);
            }
        }

        public async Task<ServiceResponse<Class>> GetClassById(int id)
        {
            try
            {
                string query = @"
                SELECT * FROM tblClass
                WHERE ClassId = @Id";

                var classObj = await _connection.QueryFirstOrDefaultAsync<Class>(query, new { Id = id });

                if (classObj != null)
                {
                    return new ServiceResponse<Class>(true, "Record Found", classObj, 200);
                }
                else
                {
                    return new ServiceResponse<Class>(false, "Record not Found", new Class(), 500);
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
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
