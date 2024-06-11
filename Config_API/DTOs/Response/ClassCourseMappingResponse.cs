namespace Config_API.DTOs.Response
{
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
}
