using System.ComponentModel.DataAnnotations;

namespace UserManagement_API.Models
{
    public class GenRefBankDetail
    {
        public int refBankID { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int referenceLinkID { get; set; }
        [Required(ErrorMessage = "Bank name cannot be empty")]
        public string BankName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Acc number cannot be empty")]
        public string ACNo { get; set; } = string.Empty;
        [Required(ErrorMessage = "IFSC code cannot be empty")]
        public string IFSC { get; set; } = string.Empty;
        public int? ReferenceID { get; set; }
    }
}
