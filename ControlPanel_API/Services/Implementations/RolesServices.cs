﻿using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class RolesServices : IRolesServices
    {
        private readonly IRolesRepository _rolesRepository;

        public RolesServices(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }
        public async Task<ServiceResponse<string>> AddRole(Role role)
        {
            try
            {
                return await _rolesRepository.AddRole(role);
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
                return await _rolesRepository.GetRoleByID(roleId);
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
                return await _rolesRepository.GetRoles();
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
                return await _rolesRepository.AddRole(role);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}