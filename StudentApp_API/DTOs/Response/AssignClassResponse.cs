namespace StudentApp_API.DTOs.Responses
{
    public class AssignClassResponse
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public bool IsClassAssigned { get; set; }
        public string Message { get; set; }
    }
    public class RegistrationDTO
    {
        public int RegistrationID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CountryCodeID { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public int CountryID { get; set; }
        public int StatusID { get; set; }
        public string Location { get; set; }
        public string ReferralCode { get; set; }
        public string SchoolCode { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsTermsAgreed { get; set; }
        public string Photo { get; set; }
      //  public string OTP { get; set; }
        public List<ParentDTO> Parents { get; set; }
    }

    public class ParentDTO
    {
        public int ParentID { get; set; }
        public string ParentType { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public int RegistrationID { get; set; }
    }

}
