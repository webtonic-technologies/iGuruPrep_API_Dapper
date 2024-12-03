using System.ComponentModel.DataAnnotations;
namespace StudentApp_API.DTOs.Requests
{
    public class RegistrationRequest
    {
        public int RegistrationId {  get; set; }
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Country code is required.")]
        public string CountryCodeID { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        public string MobileNumber { get; set; }
        public int StateId {  get; set; }

        [Required(ErrorMessage = "Email ID is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        public int CountryID { get; set; }
        public string Location { get; set; }
        public string ReferralCode { get; set; }
        public string SchoolCode { get; set; }
        public bool IsTermsAgreed { get; set; }
        public string Photo { get; set; }
    }

    public class UpdateProfileRequest
    {
        public int RegistrationID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCodeID { get; set; }
        public string MobileNumber { get; set; }
        public int StateId { get; set; }
        public string EmailID { get; set; }
        public int CountryID { get; set; }
        public string Location { get; set; }
        public string ReferralCode { get; set; }
        public string SchoolCode { get; set; }
        public string Photo { get; set; }
        public List<ParentRequest>? ParentRequests { get; set; }
    }
    public class ParentRequest
    {
        public int ParentID { get; set; }  // For update scenarios
        public string ParentType { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
    }
    public class ParentsInfo
    {
        public string ParentType { get; set; }
        public string MobileNo { get; set; }
        public string ParentEmailID { get; set; }
    }
}