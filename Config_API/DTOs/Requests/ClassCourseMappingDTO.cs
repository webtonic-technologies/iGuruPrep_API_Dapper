using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.DTOs.Requests
{
    public class ClassCourseMappingDTO
    {
        public int CourseClassMappingID { get; set; }
        [Required(ErrorMessage = "Courses cannot be empty")]
        public List<int>? CourseID { get; set; }
        [Required(ErrorMessage = "Class cannot be empty")]
        public int? ClassID { get; set; }
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        // public string EmpFirstName { get; set; } = string.Empty;
        // public string classname { get; set; } = string.Empty;
        //public string coursename { get; set; } = string.Empty;
    }
    public class GetAllClassCourseRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
