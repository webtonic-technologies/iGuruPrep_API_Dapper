using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IRolesRepository
    {
        Task<ServiceResponse<List<Role>>> GetRoles(GetAllRolesRequest request);
        Task<ServiceResponse<List<Role>>> GetRolesMasters();
        Task<ServiceResponse<Role>> GetRoleByID(int roleId);
        Task<ServiceResponse<string>> AddUpdateRole(Role role);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
