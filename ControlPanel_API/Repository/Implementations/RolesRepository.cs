﻿using ControlPanel_API.DTOs.ServiceResponse;
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
        public async Task<ServiceResponse<string>> AddRole(Role request)
        {
            try
            {
                var sql = @"
            INSERT INTO tblRole (RoleName, RoleCode, RoleNumber, Status)
            VALUES (@RoleName, @RoleCode, @RoleNumber, @Status);";

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

        public async Task<ServiceResponse<List<Role>>> GetRoles()
        {
            try
            {
                var sql = "SELECT * FROM tblRole;";
                var roles = await _connection.QueryAsync<Role>(sql);

                if (roles != null)
                {
                    return new ServiceResponse<List<Role>>(true, "Records Found", roles.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Role>>(false, "Records Not Found", new List<Role>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Role>>(false, ex.Message, new List<Role>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateRole(Role role)
        {
            try
            {
                var sql = @"
            UPDATE tblRole 
            SET RoleName = @RoleName, RoleCode = @RoleCode, RoleNumber = @RoleNumber, Status = @Status
            WHERE RoleId = @RoleId;";


                int rowsAffected = await _connection.ExecuteAsync(sql, role);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Role Updated Successfully", 200);
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
    }
}