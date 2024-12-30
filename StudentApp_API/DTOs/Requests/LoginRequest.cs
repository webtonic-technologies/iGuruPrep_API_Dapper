namespace StudentApp_API.DTOs.Requests
{
    public class LoginRequest
    {
        public string EmailIDOrPhoneNumberOrLicense { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; } = string.Empty; // Device identifier (could be UUID, device name, etc.)
        public string DeviceDetails { get; set; } = string.Empty; // Additional device information (optional)
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
