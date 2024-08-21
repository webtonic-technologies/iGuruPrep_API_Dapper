namespace Course_API.DTOs.Response
{
    public class ContentMasterResponseDTO
    {
        public int contentid { get; set; }
        public int boardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public int classId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int courseId { get; set; }
        public string CourseName { get; set; } = string.Empty; 
        public int subjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string PDF { get; set; } = string.Empty;
        public string Video { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
        public int APId { get; set; }
        public string APName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
    }
}
