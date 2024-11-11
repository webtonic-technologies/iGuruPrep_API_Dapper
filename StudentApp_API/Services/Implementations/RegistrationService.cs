using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Implementations
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationRepository _registrationRepository;

        public RegistrationService(IRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        public async Task<ServiceResponse<int>> RegisterStudentAsync(RegistrationRequest request)
        {
            return await _registrationRepository.AddRegistrationAsync(request);
        }

        public async Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request)
        {
            return await _registrationRepository.SendOTPAsync(request);
        }

        public async Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request)
        {
            return await _registrationRepository.VerifyOTPAsync(request);
        }

        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            return await _registrationRepository.LoginAsync(request);
        }
         
        public async Task<ServiceResponse<AssignCourseResponse>> AssignCourseAsync(AssignCourseRequest request)
        {
            return await _registrationRepository.AssignCourseAsync(request);
        }
        public async Task<ServiceResponse<AssignClassResponse>> AssignClassAsync(AssignClassRequest request)
        {
            return await _registrationRepository.AssignClassAsync(request);
        }
    }
}
