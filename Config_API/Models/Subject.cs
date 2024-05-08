namespace iGuruPrep.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
        public string colorcode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? displayorder { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string groupname { get; set; } = string.Empty;
        public int? subjecttype { get; set; }
        public int? EmployeeID {  get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}