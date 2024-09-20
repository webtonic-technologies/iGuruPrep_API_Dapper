namespace UserManagement_API.DTOs.Response
{
    public class GenerateReferenceResponseDTO
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
        public GenRefBankDetailResponse? GenRefBankdetail { get; set; }
        public List<ReferenceLinksResposne>? ReferenceLinksResposnes {  get; set; }
    }
    public class GenRefBankDetailResponse
    {
        public int refBankID { get; set; }
        public int referenceLinkID { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int BankId {  get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ACNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public int? ReferenceID { get; set; }
    }
    public class ReferenceLinksResposne
    {
        public int RefLinksId {  get; set; }
        public int referenceLinkID { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
        public string ReferralLink { get; set; } = string.Empty;
    }
}