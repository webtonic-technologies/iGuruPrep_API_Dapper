using ControlPanel_API.DTOs;
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

        public Task<ServiceResponse<List<RoleAssignmentMapping>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request)
        {
            throw new NotImplementedException();
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
