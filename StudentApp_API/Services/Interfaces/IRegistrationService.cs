using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<ServiceResponse<int>> RegisterStudentAsync(RegistrationRequest request);
        Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request);
        Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResponse<int>> AddUpdateProfile(UpdateProfileRequest request);
        Task<ServiceResponse<RegistrationDTO>> GetRegistrationByIdAsync(int registrationId);
        Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request);
        Task<ServiceResponse<AssignStudentMappingResponse>> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request);
        Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request);
        Task<ServiceResponse<string>> DeleteProfile(int registrationId);
        Task<ServiceResponse<List<CountryResponse>>> GetCountries();
        Task<ServiceResponse<List<StateResponse>>> GetStatesByCountryId(int countryId);
        Task<ServiceResponse<string>> UserLogout(UserLogoutRequest request);
        Task<ServiceResponse<ForgetPasswordResponse>> ForgetPasswordAsync(string userInput);
        Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ServiceResponse<bool>> VerifyOtpAndUpdateEmailAsync(VerifyEmailOtpRequest request);
        Task<ServiceResponse<bool>> VerifyOtpAndUpdateMobileAsync(VerifyMobileOtpRequest request);
        Task<ServiceResponse<SendOTPResponse>> ChangeEmailAsync(ChangeEmailRequest request);
        Task<ServiceResponse<SendOTPResponse>> ChangeMobileAsync(ChangeMobileRequest request);
        Task<ServiceResponse<string>> GmailLogin(GmailLoginRequest request);
        Task<ServiceResponse<string>> HandleMultiDeviceLoginAsync(int userId, string deviceToken, bool isCancel);
    }
}
