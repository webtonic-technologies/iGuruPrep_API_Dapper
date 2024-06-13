using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IRoleAssignmentRepository
    {
        Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId);
        Task<ServiceResponse<string>> RemoveRoleAssignment(int RAMappingId);
        Task<ServiceResponse<List<MenuMasterDTOResponse>>> GetMasterMenu();
        Task<ServiceResponse<List<RoleAssignmentResponse>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request);
        Task<ServiceResponse<RoleAssignmentResponse>> GetRoleAssignmentById(int EmployeeId);
    }
}
