namespace Course_API.Models
{
    public class Syllabus
    {
        public int SyllabusId { get; set; }
        public int BoardID { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public string SyllabusName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string? createdby { get; set; }
        public DateTime? createdon { get; set; }
        public string? modifiedby { get; set; }
        public DateTime? modifiedon { get; set; }
        public int? SubjectId { get; set; }
        public int? APID { get; set; }
        public int? empid {  get; set; }
        public string villagename {  get; set; } = string.Empty;
        public string DesignationName {  get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string boardname { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
        public string coursename { get; set; } = string.Empty;
        public string subjectname { get; set; } = string.Empty;
        public string APname { get; set; } = string.Empty;
        public int? EmployeeID {  get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}