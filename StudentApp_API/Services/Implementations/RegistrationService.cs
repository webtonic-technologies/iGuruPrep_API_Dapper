using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
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
 
        public async Task<ServiceResponse<int>> AddUpdateProfile(UpdateProfileRequest request)
        {
            return await _registrationRepository.AddUpdateProfile(request);
        }

        public async Task<ServiceResponse<RegistrationDTO>> GetRegistrationByIdAsync(int registrationId)
        {
            return await _registrationRepository.GetRegistrationByIdAsync(registrationId);
        }

        public async Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request)
        {
            return await _registrationRepository.DeviceCapture(request);
        }

        public async Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request)
        {
            return await _registrationRepository.GetAllClassCoursesMappings(request);
        }

        public async Task<ServiceResponse<AssignStudentMappingResponse>> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request)
        {
            return await _registrationRepository.AssignStudentClassCourseBoardMapping(request);
        }

        public async Task<ServiceResponse<string>> DeleteProfile(int registrationId)
        {
            return await _registrationRepository.DeleteProfile(registrationId);
        }

        public async Task<ServiceResponse<List<CountryResponse>>> GetCountries()
        {
            return await _registrationRepository.GetCountries();
        }

        public async Task<ServiceResponse<List<StateResponse>>> GetStatesByCountryId(int countryId)
        {
            return await _registrationRepository.GetStatesByCountryId(countryId);
        }

        public async Task<ServiceResponse<string>> UserLogout(UserLogoutRequest request)
        {
            return await _registrationRepository.UserLogout(request);
        }

        public async Task<ServiceResponse<ForgetPasswordResponse>> ForgetPasswordAsync(string userInput)
        {
            return await _registrationRepository.ForgetPasswordAsync(userInput);
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return await _registrationRepository.ResetPasswordAsync(request);
        }
    }
}
