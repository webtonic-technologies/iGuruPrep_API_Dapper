using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<ServiceResponse<int>> AddRegistrationAsync(RegistrationRequest request);
        Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request);
        Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request);
        Task<ServiceResponse<AssignStudentMappingResponse>> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request);
        Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request);
        Task<ServiceResponse<int>> AddUpdateProfile(UpdateProfileRequest request);
        Task<ServiceResponse<RegistrationDTO>> GetRegistrationByIdAsync(int registrationId);
        Task<ServiceResponse<string>> DeleteProfile(int registrationId);
        Task<ServiceResponse<List<CountryResponse>>> GetCountries();
        Task<ServiceResponse<List<StateResponse>>> GetStatesByCountryId(int countryId);
        Task<ServiceResponse<string>> UserLogout(UserLogoutRequest request);
        Task<ServiceResponse<ForgetPasswordResponse>> ForgetPasswordAsync(string userInput);
    }
}
