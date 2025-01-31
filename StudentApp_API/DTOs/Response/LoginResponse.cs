namespace StudentApp_API.DTOs.Responses
{
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public string Location { get; set; }
        public bool IsLoginSuccessful { get; set; }
        public bool IsTermsAgreed {  get; set; }
        public bool IsEmployee {  get; set; }
        public string Role {  get; set; }
        public string ProfilePercentage { get; set; } = string.Empty;
        public LicenseDetails? LicenseDetails {  get; set; }
    }
    public class LicenseDetails
    {
        public string SchoolCode { get; set; }
        public string ReferralCode {  get; set; }
    }
    public class ClassCourseMappingResponse
    {
        public List<CourseData>? Courses { get; set; }
        public int? ClassID { get; set; }
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public int? EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
    }
    public class CourseData
    {
        public int CourseClassMappingID { get; set; }
        public int CourseID { get; set; }
        public string Coursename { get; set; } = string.Empty;
    }
    public class AssignStudentMappingResponse
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public int BoardID { get; set; }
        public bool IsAssigned { get; set; }
        public string Message { get; set; }
    }

}
