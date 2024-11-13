namespace StudentApp_API.DTOs.Responses
{
    //public class RegistrationResponse
    //{
    //    public int RegistrationID { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string EmailID { get; set; }
    //    public string MobileNumber { get; set; }
    //    public bool IsRegistered { get; set; }
    //}
    public class RegistrationResponse
    {
        public int RegistrationID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCodeID { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public int? CountryID { get; set; }
        public int? StatusID { get; set; }
        public string Location { get; set; }
        public string ReferralCode { get; set; }
        public string SchoolCode { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsTermsAgreed { get; set; }
        public string Photo { get; set; }
        public string OTP { get; set; }

        public List<ParentResponse>? Parents { get; set; } = new List<ParentResponse>();
    }

    public class ParentResponse
    {
        public int ParentID { get; set; }
        public string ParentType { get; set; }
        public string ParentMobileNo { get; set; }
        public string ParentEmailID { get; set; }
    }

}
