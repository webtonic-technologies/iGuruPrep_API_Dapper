using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _connection;

        public EmployeeRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateEmployee(Employee request)
        {
            try
            {
                if (request.Employeeid == 0)
                {
                    string insertQuery = @"
                INSERT INTO [tblEmployee] (UserCode, RoleId, DesignationID, EmpFirstName, EmpLastName,
                                             EmpPhoneNumber, EmpEmail, EmpDOB, ZipCode, DistrictName, SubjectId,
                                             StateName, VcName, RoleName, DesignationName,
                                             CreatedOn, CreatedBy, Status)
                VALUES (@UserCode, @RoleId, @DesignationID, @EmpFirstName, @EmpLastName,
                        @EmpPhoneNumber, @EmpEmail, @EmpDOB, @ZipCode, @DistrictName, @SubjectId,
                        @StateName, @VcName, @RoleName, @DesignationName,
                        @CreatedOn, @CreatedBy, @Status);";
                    var rowsAffected = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.Usercode,
                        request.RoleID,
                        request.DesignationID,
                        request.EmpFirstName,
                        request.EmpLastName,
                        request.EMPPhoneNumber,
                        request.EMPEmail,
                        request.EMPDOB,
                        request.ZipCode,
                        request.DistrictName,
                        request.SubjectID,
                        request.StateName,
                        request.VcName,
                        request.Rolename,
                        request.Designationname,
                        request.Createdby,
                        CreatedOn = DateTime.Now,
                        Status = true
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Employee added successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
                UPDATE [tblEmployee]
                SET UserCode = @UserCode,
                    RoleId = @RoleId,
                    DesignationId = @DesignationId,
                    EmpFirstName = @EmpFirstName,
                    EmpLastName = @EmpLastName,
                    EmpPhoneNumber = @EmpPhoneNumber,
                    EmpEmail = @EmpEmail,
                    EmpDOB = @EmpDOB,
                    ZipCode = @ZipCode,
                    DistrictName = @DistrictName,
                    SubjectId = @SubjectId,
                    StateName = @StateName,
                    VcName = @VcName,
                    RoleName = @RoleName,
                    DesignationName = @DesignationName,
                    ModifiedOn = @ModifiedOn,
                    ModifiedBy = @ModifiedBy,
                    Status = @Status
                WHERE Employeeid = @Employeeid;";

                    var rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.Employeeid,
                        request.Usercode,
                        request.RoleID,
                        request.DesignationID,
                        request.EmpFirstName,
                        request.EmpLastName,
                        request.EMPPhoneNumber,
                        request.EMPEmail,
                        request.EMPDOB,
                        request.ZipCode,
                        request.DistrictName,
                        request.SubjectID,
                        request.StateName,
                        request.VcName,
                        request.Rolename,
                        request.Designationname,
                        request.Modifiedby,
                        ModifiedOn = DateTime.Now,
                        request.Status
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Employee updated successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<Employee>> GetEmployeeByID(int Id)
        {
            try
            {
                string query = @"
                SELECT *
                FROM [tblEmployee]
                WHERE [Employeeid] = @EmployeeId";

                var data = await _connection.QueryFirstOrDefaultAsync<Employee>(query, new { EmployeeId = Id });
                if (data != null)
                {
                    return new ServiceResponse<Employee>(true, "Records found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Employee>(false, "Records not found", new Employee(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Employee>(false, ex.Message, new Employee(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Employee>>> GetEmployeeList(GetEmployeeListDTO request)
        {
            try
            {
                string query = @"
                SELECT *
                FROM [tblEmployee]
                WHERE 1 = 1";

                // Add filters based on DTO properties
                if (request.RoleId > 0)
                {
                    query += " AND [RoleID] = @RoleId";
                }
                if (request.Designation > 0)
                {
                    query += " AND [DesignationID] = @DesignationId";
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    query += " AND ([EmpFirstName] LIKE @SearchText OR [EmpLastName] LIKE @SearchText)";
                    request.SearchText = "%" + request.SearchText + "%";
                }
                var data = await _connection.QueryAsync<Employee>(query, request);
                if (data != null)
                {
                    return new ServiceResponse<List<Employee>>(true, "Records found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Employee>>(false, "Records not found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Employee>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetEmployeeByID(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status;

                    string sql = "UPDATE [tblEmployee] SET Status = @Status WHERE [Employeeid] = @Employeeid";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { data.Data.Status, Employeeid = id });
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
