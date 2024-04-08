namespace UserManagement_API.Models
{
    public class GenRefBankDetail
    {
        public int refBankID { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int referenceLinkID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ACNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public int? ReferenceID { get; set; }
    }
}
