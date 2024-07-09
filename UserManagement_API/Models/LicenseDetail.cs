namespace UserManagement_API.Models
{
    public class LicenseDetail
    {
        public int LicenseDetailID { get; set; }
        public int? GenerateLicenseID { get; set; }
        public int? BoardID { get; set; }
        public int? ClassID { get; set; }
        public int? CourseID { get; set; }
        public int NoOfLicense { get; set; }
        public int ValidityID { get; set; }
        public int APID { get; set; }
        public int ExamTypeId {  get; set; }
        //public string BoardName { get; set; } = string.Empty;
        //public string ClassName { get; set; } = string.Empty;
        //public string CourseName { get; set; } = string.Empty;
        //public string CategoryName { get; set; } = string.Empty;
    }
}