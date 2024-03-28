namespace Schools_API.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string? PathURL { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public int BoardId { get; set; }
        public int SubjectId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon {  get; set; }
    }
}
