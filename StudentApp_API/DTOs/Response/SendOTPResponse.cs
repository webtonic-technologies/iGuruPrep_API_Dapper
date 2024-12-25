namespace StudentApp_API.DTOs.Responses
{
    public class SendOTPResponse
    {
        public int RegistrationID { get; set; }
        public string OTP { get; set; }
        public bool IsOTPSent { get; set; }
    }
    public class StateResponse
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int Status { get; set; }
    }
    public class CountryResponse
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int CountryCode { get; set; }
    }
}
