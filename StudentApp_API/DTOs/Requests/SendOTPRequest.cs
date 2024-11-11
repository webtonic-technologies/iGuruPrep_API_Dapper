namespace StudentApp_API.DTOs.Requests
{
    public class SendOTPRequest
    {
        public int RegistrationID { get; set; }
        public string MobileNumber { get; set; }
    }
}
