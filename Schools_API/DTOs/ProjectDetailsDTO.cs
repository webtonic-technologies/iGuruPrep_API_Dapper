namespace Schools_API.DTOs
{
    public class ProjectDetailsDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
    }
}
