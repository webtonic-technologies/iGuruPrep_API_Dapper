namespace StudentApp_API.DTOs.Requests
{
    public class LoginRequest
    {
        public string EmailID { get; set; }
        public string Password { get; set; }
    }
    public class GetAllClassCourseRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class AssignStudentMappingRequest
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public int BoardID { get; set; }
    }

}
