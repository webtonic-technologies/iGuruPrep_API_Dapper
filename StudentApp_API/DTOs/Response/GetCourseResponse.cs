namespace StudentApp_API.DTOs.Responses
{
    public class GetCourseResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public bool Status { get; set; }
    }
}
