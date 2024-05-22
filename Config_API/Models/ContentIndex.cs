namespace Config_API.Models
{
    public class ContentIndex
    {
        public int SubjectIndexId { get; set; }
        public int SubjectId { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public int ParentLevel { get; set; }
        public bool? Status { get; set; }
        public int classid { get; set; }
        public int boardid { get; set; }
        public int APID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public int courseid { get; set; }
        public string pathURL { get; set; } = string.Empty;
        public string APName { get; set; } = string.Empty;
        public string Subjectname { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}
