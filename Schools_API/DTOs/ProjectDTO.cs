namespace Schools_API.DTOs
{
    public class ProjectDTO
    {
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string? PathURL { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public int BoardId { get; set; }
        public int SubjectId { get; set; }
        public string CreatedBy { get; set; }
        public IFormFile? image { get; set; }
        public string ReferenceLink { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
    }
}
