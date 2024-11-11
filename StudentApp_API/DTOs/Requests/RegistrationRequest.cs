namespace StudentApp_API.DTOs.Requests
{
    public class RegistrationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCodeID { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public int CountryID { get; set; }
        public string Location { get; set; }
        public string ReferralCode { get; set; }
        public string SchoolCode { get; set; }
        public bool IsTermsAgreed { get; set; }
        public string Photo { get; set; }
    }
}
