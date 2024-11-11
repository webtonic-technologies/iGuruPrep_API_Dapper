namespace StudentApp_API.DTOs.Requests
{
    public class VerifyOTPRequest
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
    }
}
