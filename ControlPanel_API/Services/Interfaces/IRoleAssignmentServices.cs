using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IRoleAssignmentServices
    {
        Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId);
        Task<ServiceResponse<string>> RemoveRoleAssignment(int RAMappingId);
        Task<ServiceResponse<List<MenuMasterDTOResponse>>> GetMasterMenu();
        Task<ServiceResponse<List<RoleAssignmentMapping>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request);
    }
}
