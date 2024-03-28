namespace Schools_API.DTOs
{
    public class ProjectDetailsDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string PathURL { get; set; }
        public string CourseName { get; set; }
        public string ClassName { get; set; }
        public string BoardName { get; set; }
        public string SubjectName { get; set; }
        public string CreatedBy { get; set; }
        public string ImageName { get; set; }
    }
}
