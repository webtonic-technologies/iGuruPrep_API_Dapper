namespace StudentApp_API.DTOs.Requests
{
    public class VerifyOTPRequest
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
    }
    public class DeviceCaptureRequest
    {
        public int DeviceCaptureId { get; set; }
        public int UserId { get; set; }
        public string device { get; set; } = string.Empty;
        public string fingerprint { get; set; } = string.Empty;
        public string model { get; set; } = string.Empty;
        public string serialNumber { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string version_sdkInt { get; set; } = string.Empty;
        public string version_securityPatch { get; set; } = string.Empty;
        public string id_buildId { get; set; } = string.Empty;
        public bool isPhysicalDevice { get; set; }
        public string systemName { get; set; } = string.Empty;
        public string systemVersion { get; set; } = string.Empty;
        public string utsname_version { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string browserName { get; set; } = string.Empty;
        public string appName { get; set; } = string.Empty;
        public string appVersion { get; set; } = string.Empty;
        public string deviceMemory { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string kernelVersion { get; set; } = string.Empty;
        public string computerName { get; set; } = string.Empty;
        public string systemGUID { get; set; } = string.Empty;
    }
    public class UserLogoutRequest
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public bool IsEmployee {  get; set; }
    }
    public class UserSession
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }
        public bool IsActive { get; set; }
    }
    public class ResetPasswordRequest
    {
        public int UserId { get; set; }
      //  public string UserName { get; set; }
        public string UserType { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword {  get; set; }
    }
}
