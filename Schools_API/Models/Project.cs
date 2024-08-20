namespace Schools_API.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public int BoardId { get; set; }
        public int SubjectId { get; set; }
        public string createdby { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public DateTime? createdon { get; set; }
        public int APID { get; set; }
        public string boardname { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
        public string coursename { get; set; } = string.Empty;
        public string subjectname { get; set; } = string.Empty;
        public string APname { get; set; } = string.Empty;
        public string EmpFirstName { get; set; } = string.Empty;
        public string pdfVideoFile { get; set; } = string.Empty;
    }
}
