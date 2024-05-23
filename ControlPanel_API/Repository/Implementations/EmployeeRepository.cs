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
        public async Task<ServiceResponse<string>> AddUpdateEmployee(EmployeeDTO request)
        {
            try
            {
                if (request.Employeeid == 0)
                {
                    string insertQuery = @"
                INSERT INTO [tblEmployee] (UserCode, RoleId, DesignationID, EmpFirstName, EmpLastName,
                                             EmpPhoneNumber, EmpEmail, EmpDOB, ZipCode, DistrictName,
                                             StateName, VcName, RoleName, DesignationName,
                                             CreatedOn, CreatedBy, Status)
                VALUES (@UserCode, @RoleId, @DesignationID, @EmpFirstName, @EmpLastName,
                        @EmpPhoneNumber, @EmpEmail, @EmpDOB, @ZipCode, @DistrictName,
                        @StateName, @VcName, @RoleName, @DesignationName,
                        @CreatedOn, @CreatedBy, @Status);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    var insertedValue = await _connection.QueryFirstOrDefaultAsync<int>(insertQuery, new
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
                        //request.SubjectID,
                        request.StateName,
                        request.VcName,
                        request.Rolename,
                        request.Designationname,
                        request.Createdby,
                        CreatedOn = DateTime.Now,
                        Status = true
                    });
                    if (insertedValue > 0)
                    {
                        int empSub = EmployeeSubjectMapping(request.EmployeeSubjects ??= ([]), insertedValue);
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
                        //request.SubjectID,
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
        public async Task<ServiceResponse<EmployeeDTO>> GetEmployeeByID(int Id)
        {
            try
            {
                EmployeeDTO response = new();
                string query = @"
                SELECT *
                FROM [tblEmployee]
                WHERE [Employeeid] = @EmployeeId";

                var data = await _connection.QueryFirstOrDefaultAsync<Employee>(query, new { EmployeeId = Id });
                if (data != null)
                {
                    response.Employeeid = data.Employeeid;
                    response.Usercode = data.Usercode;
                    response.RoleID = data.RoleID;
                    response.DesignationID = data.DesignationID;
                    response.EmpFirstName = data.EmpFirstName;
                    response.EmpLastName = data.EmpLastName;
                    response.EMPPhoneNumber = data.EMPPhoneNumber;
                    response.EMPEmail = data.EMPEmail;
                    response.EMPDOB = data.EMPDOB;
                    response.ZipCode = data.ZipCode;
                    response.DistrictName = data.DistrictName;
                    response.StateName = data.StateName;
                    response.VcName = data.VcName;
                    response.Rolename = data.Rolename;
                    response.Designationname = data.Designationname;
                    response.Modifiedon = data.Modifiedon;
                    response.Modifiedby = data.Modifiedby;
                    response.Createdon = data.Createdon;
                    response.Createdby = data.Createdby;
                    response.Status = data.Status;
                    response.EmployeeSubjects = GetListOfEmployeeSubject(Id);
                    return new ServiceResponse<EmployeeDTO>(true, "Records found", response, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<EmployeeDTO>(false, "Records not found", new EmployeeDTO(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<EmployeeDTO>(false, ex.Message, new EmployeeDTO(), StatusCodes.Status500InternalServerError);
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
                if (request.DesignationId > 0)
                {
                    query += " AND [DesignationID] = @DesignationId";
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    query += " AND ([EmpFirstName] LIKE @SearchText OR [EmpLastName] LIKE @SearchText)";
                    request.SearchText = "%" + request.SearchText + "%";
                }
                var data = await _connection.QueryAsync<Employee>(query, request);
                if (data.Any())
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
        private List<EmployeeSubject> GetListOfEmployeeSubject(int EmployeeId)
        {
            var query = @"SELECT * FROM [tblEmployeeSubject] WHERE  [Employeeid] = @Employeeid;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<EmployeeSubject>(query, new { Employeeid = EmployeeId });
            return data != null ? data.AsList() : [];
        }
        private int EmployeeSubjectMapping(List<EmployeeSubject> request, int EmployeeId)
        {
            foreach (var data in request)
            {
                data.Employeeid = EmployeeId;
            }
            string query = "SELECT COUNT(*) FROM [tblEmployeeSubject] WHERE [Employeeid] = @Employeeid";
            int count = _connection.QueryFirstOrDefault<int>(query, new { Employeeid = EmployeeId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblEmployeeSubject]
                          WHERE [Employeeid] = @Employeeid;";
                var rowsAffected = _connection.Execute(deleteDuery, new { Employeeid = EmployeeId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblEmployeeSubject] ([SubjectID], [SubjectName], [Employeeid])
                          VALUES (@SubjectID, @SubjectName, @Employeeid);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblEmployeeSubject] ([SubjectID], [SubjectName], [Employeeid])
                          VALUES (@SubjectID, @SubjectName, @Employeeid);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
    }
}
