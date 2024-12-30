using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class EmployeeServices : IEmployeeServices
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeeServices(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateEmployee(EmployeeDTO request)
        {
            try
            {
                return await _employeeRepository.AddUpdateEmployee(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        //public async Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request)
        //{
        //    try
        //    {
        //        return await _employeeRepository.DeviceCapture(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //}

        public async Task<ServiceResponse<EmployeeLoginResponse>> EmployeeLogin(EmployeeLoginRequest request)
        {
            try
            {
                return await _employeeRepository.EmployeeLogin(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<EmployeeLoginResponse>(false, ex.Message, new EmployeeLoginResponse(), 500);
            }
        }

        public async Task<ServiceResponse<EmployeeResponseDTO>> GetEmployeeByID(int ID)
        {
            try
            {
                return await _employeeRepository.GetEmployeeByID(ID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<EmployeeResponseDTO>(false, ex.Message, new EmployeeResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<EmployeeResponseDTO>>> GetEmployeeList(GetEmployeeListDTO request)
        {
            try
            {
                return await _employeeRepository.GetEmployeeList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<EmployeeResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _employeeRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        //public async Task<ServiceResponse<string>> UserLogin(UserLoginRequest request)
        //{
        //    try
        //    {
        //        return await _employeeRepository.UserLogin(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //}

        //public async Task<ServiceResponse<string>> UserLogout(UserLogoutRequest request)
        //{
        //    try
        //    {
        //        return await _employeeRepository.UserLogout(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //}
    }
}
