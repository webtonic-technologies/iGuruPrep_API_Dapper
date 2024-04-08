namespace UserManagement_API.Models
{
    public class GenerateReference
    {
        public int referenceLinkID { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionBranchName { get; set; } = string.Empty;
        public string InstitutionBranchCode { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public int? StateId { get; set; }
        public int? DistrictID { get; set; }
        public string PAN { get; set; } = string.Empty;
        public int? ReferenceID { get; set; }
        public string PersonName { get; set; } = string.Empty;
    }
}
