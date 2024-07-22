using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<ServiceResponse<List<EmployeeResponseDTO>>> GetEmployeeList(GetEmployeeListDTO request);
        Task<ServiceResponse<EmployeeResponseDTO>> GetEmployeeByID(int ID);
        Task<ServiceResponse<string>> AddUpdateEmployee(EmployeeDTO request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<EmployeeLoginResponse>> EmployeeLogin(EmployeeLoginRequest request);
    }
}
