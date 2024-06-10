using System.ComponentModel.DataAnnotations;
using UserManagement_API.Models;

namespace UserManagement_API.DTOs
{
    public class GenerateReferenceDTO
    {
        public int referenceLinkID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int NumberOfRef { get; set; }
        [Required(ErrorMessage = "Mobile number cannot be empty")]
        public string MobileNo { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty")]
        public string EmailID { get; set; } = string.Empty;
        public int? StateId { get; set; }
        public int? DistrictID { get; set; }
        public string PAN { get; set; } = string.Empty;
        public int? ReferenceID { get; set; }
        [Required(ErrorMessage = "Person name cannot be empty")]
        public string PersonName { get; set; } = string.Empty;
        public GenRefBankDetail? GenRefBankdetail { get; set; }
    }
    public class GenerateReferenceListDTO
    {
        public int referenceLinkID { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
    }
    public class GetAllReferralsRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
