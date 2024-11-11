namespace StudentApp_API.DTOs.Responses
{
    public class GetClassCourseResponse
    {
        public int CourseClassMappingID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
    }
}
