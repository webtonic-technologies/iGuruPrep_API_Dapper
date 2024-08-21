namespace UserManagement_API.Models
{
    public class GenerateLicense
    {
        public int GenerateLicenseID { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string ChairmanEmail { get; set; } = string.Empty;
        public string ChairmanMobile { get; set; } = string.Empty;
        public string PrincipalEmail { get; set; } = string.Empty;
        public string PrincipalMobile { get; set; } = string.Empty;
        public int? stateid { get; set; }
        public int? DistrictID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string ChairmanUsername { get; set; } = string.Empty;
        public string ChairmanPassword { get; set; } = string.Empty;
        public string PrincipalUsername { get; set; } = string.Empty;
        public string PrincipalPassword { get; set; } = string.Empty;
    }
}