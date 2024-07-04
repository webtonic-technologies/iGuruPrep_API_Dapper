using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IRoleAssignmentServices
    {
        Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request);
        Task<ServiceResponse<string>> RemoveRoleAssignment(int RAMappingId, int roleId, int designationId);
        Task<ServiceResponse<List<MenuMasterDTOResponse>>> GetMasterMenu();
        Task<ServiceResponse<List<RoleAssignmentResponse>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request);
        Task<ServiceResponse<RoleAssignmentResponse>> GetRoleAssignmentById(int EmployeeId);
    }
}
