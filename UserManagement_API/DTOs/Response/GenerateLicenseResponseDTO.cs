namespace UserManagement_API.DTOs.Response
{
    public class GenerateLicenseResponseDTO
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
        public DateTime? modifiedon { get; set; }
        public string? modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<LicenseDetailResponse>? LicenseDetails { get; set; }
    }
    public class LicenseDetailResponse
    {
        public int LicenseDetailID { get; set; }
        public int? GenerateLicenseID { get; set; }
        public int? BoardID { get; set; }
        public int? ClassID { get; set; }
        public int? CourseID { get; set; }
        public int? NoOfLicense { get; set; }
        public int ValidityID { get; set; }
        public int APID { get; set; }
        public int ExamTypeId {  get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ValidityName { get; set; } = string.Empty;
    }
}
