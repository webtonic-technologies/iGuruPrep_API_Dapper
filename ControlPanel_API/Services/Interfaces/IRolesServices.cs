using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IRolesServices
    {
        Task<ServiceResponse<List<Role>>> GetRoles(GetAllRolesRequest request);
        Task<ServiceResponse<List<Role>>> GetRolesMasters();
        Task<ServiceResponse<Role>> GetRoleByID(int roleId);
        Task<ServiceResponse<string>> AddRole(Role role);
        Task<ServiceResponse<string>> UpdateRole(Role role);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
