namespace StudentApp_API.DTOs.Responses
{
    public class GetCourseResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public bool Status { get; set; }
    }
    public class GetClassesResponse
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string CLassCode { get; set; }
        public bool Status { get; set; }
    }
    public class GetBoardsResponse
    {
        public int BoardId { get; set; }
        public string BoardName { get; set; }
        public string BoardCode { get; set; }
        public bool Status { get; set; }
    }
}
