namespace StudentApp_API.DTOs.Responses
{
    public class VerifyOTPResponse
    {
        public int RegistrationID { get; set; }
        public bool IsVerified { get; set; }
        public string Message { get; set; }
    }
}
