namespace UserManagement_API.Models
{
    public class LicenseDetail
    {
        public int LicenseDetailID { get; set; }
        public int? GenerateLicenseID { get; set; }
        public int? BoardID { get; set; }
        public int? ClassID { get; set; }
        public int? CourseID { get; set; }
        public int? NoOfLicense { get; set; }
        public string Validity { get; set; } = string.Empty;
    }
}
