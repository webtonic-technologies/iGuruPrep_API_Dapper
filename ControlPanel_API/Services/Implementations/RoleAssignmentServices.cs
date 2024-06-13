using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class RoleAssignmentServices : IRoleAssignmentServices
    {
        private readonly IRoleAssignmentRepository _roleAssignmentRepository;

        public RoleAssignmentServices(IRoleAssignmentRepository roleAssignmentRepository)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId)
        {
            try
            {
                return await _roleAssignmentRepository.AddUpdateRoleAssignment(request, EmployeeId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<RoleAssignmentResponse>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request)
        {

            try
            {
                return await _roleAssignmentRepository.GetListOfRoleAssignment(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RoleAssignmentResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<MenuMasterDTOResponse>>> GetMasterMenu()
        {
            try
            {
                return await _roleAssignmentRepository.GetMasterMenu();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MenuMasterDTOResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<RoleAssignmentResponse>> GetRoleAssignmentById(int EmployeeId)
        {

            try
            {
                return await _roleAssignmentRepository.GetRoleAssignmentById(EmployeeId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoleAssignmentResponse>(false, ex.Message, new RoleAssignmentResponse(), 500);
            }
        }
        public async Task<ServiceResponse<string>> RemoveRoleAssignment(int RAMappingId)
        {

            try
            {
                return await _roleAssignmentRepository.RemoveRoleAssignment(RAMappingId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
