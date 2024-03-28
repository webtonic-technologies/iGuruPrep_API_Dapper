using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IRolesRepository
    {
        Task<ServiceResponse<List<Role>>> GetRoles();
        Task<ServiceResponse<Role>> GetRoleByID(int roleId);
        Task<ServiceResponse<string>> AddRole(Role role);
        Task<ServiceResponse<string>> UpdateRole(Role role);
    }
}
