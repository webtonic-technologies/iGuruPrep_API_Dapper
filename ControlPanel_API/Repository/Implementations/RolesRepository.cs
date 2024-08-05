using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class RolesRepository : IRolesRepository
    {
        private readonly IDbConnection _connection;

        public RolesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateRole(Role request)
        {
            try
            {
                if (request.RoleId == 0)
                {
                    var sql = @"
            INSERT INTO tblRole (RoleName, RoleCode, Status, createdon, createdby)
            VALUES (@RoleName, @RoleCode, @Status, @createdon, @createdby);";

                    int rowsAffected = await _connection.ExecuteAsync(sql, request);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Role Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var sql = @"
            UPDATE tblRole 
            SET RoleName = @RoleName, RoleCode = @RoleCode, Status = @Status,
            modifiedon = @modifiedon, modifiedby = @modifiedby
            WHERE RoleId = @RoleId;";


                    int rowsAffected = await _connection.ExecuteAsync(sql, request);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Role Updated Successfully", 200);
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
        public async Task<ServiceResponse<Role>> GetRoleByID(int roleId)
        {
            try
            {
                var sql = "SELECT * FROM tblRole WHERE RoleId = @RoleId;";
                var role = await _connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleId = roleId });
                if (role != null)
                {
                    return new ServiceResponse<Role>(true, "Records Found", role, 200);
                }
                else
                {
                    return new ServiceResponse<Role>(false, "Records Not Found", new Role(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Role>(false, ex.Message, new Role(), 500);
            }
        }
        public async Task<ServiceResponse<List<Role>>> GetRolesMasters()
        {
            try
            {
                var sql = "SELECT * FROM tblRole;";
                var roles = await _connection.QueryAsync<Role>(sql);

                if (roles.Any())
                {
                    return new ServiceResponse<List<Role>>(true, "Records Found", roles.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Role>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Role>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<Role>>> GetRoles(GetAllRolesRequest request)
        {
            try
            {
                var sql = "SELECT * FROM tblRole;";
                var roles = await _connection.QueryAsync<Role>(sql);
                var paginatedList = roles.Skip((request.PageNumber - 1) * request.PageSize)
           .Take(request.PageSize)
           .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Role>>(true, "Records Found", paginatedList.AsList(), 200, roles.Count());
                }
                else
                {
                    return new ServiceResponse<List<Role>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Role>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var role = await GetRoleByID(id);

                if (role.Data != null)
                {
                    role.Data.Status = !role.Data.Status;

                    string sql = "UPDATE [tblRole] SET Status = @Status WHERE [RoleID] = @RoleID";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { role.Data.Status, RoleID = id });
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
