namespace UserManagement_API.DTOs.Requests
{
    public class UserRegistrationDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IFormFile? Photo { get; set; } = null;
        public string SchoolCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string RoleID { get; set; } = string.Empty;
        public int? PersonType { get; set; }
    }
}
