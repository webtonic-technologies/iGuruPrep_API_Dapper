using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
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

        public async Task<ServiceResponse<EmployeeDTO>> GetEmployeeByID(int ID)
        {
            try
            {
                return await _employeeRepository.GetEmployeeByID(ID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<EmployeeDTO>(false, ex.Message, new EmployeeDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<Employee>>> GetEmployeeList(GetEmployeeListDTO request)
        {
            try
            {
                return await _employeeRepository.GetEmployeeList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Employee>>(false, ex.Message, [], 500);
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
    }
}
