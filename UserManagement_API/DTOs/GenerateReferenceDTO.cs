using UserManagement_API.Models;

namespace UserManagement_API.DTOs
{
    public class GenerateReferenceDTO
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
        public GenRefBankDetail? GenRefBankdetail { get; set; }
    }
    public class GenerateReferenceListDTO
    {
        public int referenceLinkID { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
    }
}
