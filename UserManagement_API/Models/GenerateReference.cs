namespace UserManagement_API.Models
{
    public class GenerateReference
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
    }
}