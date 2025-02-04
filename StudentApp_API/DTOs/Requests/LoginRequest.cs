using static System.Net.WebRequestMethods;

namespace StudentApp_API.DTOs.Requests
{
    public class LoginRequest
    {
        public string EmailIDOrPhoneNumberOrLicense { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; } = string.Empty; // Device identifier (could be UUID, device name, etc.)
        public string DeviceDetails { get; set; } = string.Empty; // Additional device information (optional)
    }
    public class GetAllClassCourseRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class AssignStudentMappingRequest
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public int BoardID { get; set; }
    }
    public class GmailLoginRequest
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string PhotoUrl { get; set; }
        public string ServerAuthCode { get; set; }
    }
    public class ChangeMobileRequest
    {
        public int RegistrationID { get; set; }
        public string OldMobileNumber { get; set; }
        public string NewMobileNumber { get; set; }
    }
    public class VerifyMobileOtpRequest
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
        public string NewMobileNumber { get; set; }
    }
    public class VerifyEmailOtpRequest
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
        public string NewEmail { get; set; }
    }

    public class ChangeEmailRequest
    {
        public int RegistrationID { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
    }
}
