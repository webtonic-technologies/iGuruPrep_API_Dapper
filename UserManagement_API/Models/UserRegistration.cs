using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserManagement_API.Models
{
    public class UserRegistration
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public int? PersonType { get; set; }
        public string UserCode { get; set; } = "ABCD1234";
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; } = DateTime.Today;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ZipCode { get; set; } = "41104256";
        public string Photo { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public Guid? CurrentToken { get; set; } = Guid.NewGuid();
        public string System_Name { get; set; } = string.Empty;
        public string Last_Ip_Address { get; set; } = string.Empty;
        public string Login_State { get; set; } = string.Empty;
        public DateTime? Last_Login_DateTime { get; set; }
        public DateTime? Last_Logout_DateTime { get; set; }
        public string Require_ReLogin { get; set; } = "1";
        public byte? Failed_Login_Attempts { get; set; } = 1;
        public DateTime? Cannot_Login_Until_Date { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string City { get; set; } = "Mumbai";
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string FcmDeviceID { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public byte[]? PaymentSucessHash { get; set; }
        public byte[]? PaymentFailureHash { get; set; }
        public string RoleID { get; set; } = string.Empty;
        public string DesgnID { get; set; } = string.Empty;
    }
}
