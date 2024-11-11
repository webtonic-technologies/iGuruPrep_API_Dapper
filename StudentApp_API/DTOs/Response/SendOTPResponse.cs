namespace StudentApp_API.DTOs.Responses
{
    public class SendOTPResponse
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
        public bool IsOTPSent { get; set; }
    }
}
