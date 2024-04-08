namespace UserManagement_API.Models
{
    public class GenerateLicense
    {
        public int GenerateLicenseID { get; set; }
        public int? SchoolID { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public int? BranchName { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string ChairmanEmail { get; set; } = string.Empty;
        public string ChairmanMobile { get; set; } = string.Empty;
        public string PrincipalEmail { get; set; } = string.Empty;
        public string PrincipalMobile { get; set; } = string.Empty;
        public int? stateid { get; set; }
        public int? DistrictID { get; set; }
    }
}
