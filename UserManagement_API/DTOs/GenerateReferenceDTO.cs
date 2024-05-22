using UserManagement_API.Models;

namespace UserManagement_API.DTOs
{
    public class GenerateReferenceDTO
    {
        public int referenceLinkID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int NumberOfRef { get; set; }
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
