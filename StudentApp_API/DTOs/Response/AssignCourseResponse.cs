namespace StudentApp_API.DTOs.Responses
{
    public class AssignCourseResponse
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public bool IsAssigned { get; set; }
        public string Message { get; set; }
    }
}
