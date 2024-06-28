using System.ComponentModel.DataAnnotations;
using UserManagement_API.Models;

namespace UserManagement_API.DTOs.Requests
{
    public class GenerateLicenseDTO
    {
        public int GenerateLicenseID { get; set; }
        [Required(ErrorMessage = "School name cannot be empty")]
        public string SchoolName { get; set; } = string.Empty;
        [Required(ErrorMessage = "School code cannot be empty")]
        public string SchoolCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        //public string StateName { get; set; } = string.Empty;
        //public string DistrictName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty")]
        public string ChairmanEmail { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        public string ChairmanMobile { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty")]
        public string PrincipalEmail { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        public string PrincipalMobile { get; set; } = string.Empty;
        public int? stateid { get; set; }
        public int? DistrictID { get; set; }
        //public string ClassName { get; set; } = string.Empty;
        //public string CourseName { get; set; } = string.Empty;
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
        public List<LicenseDetail>? LicenseDetails { get; set; }
    }
    public class GetAllLicensesListRequest
    {
        public int StateId { get; set; }
        public int District { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchText { get; set; } = string.Empty;
    }
}
