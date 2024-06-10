using System.ComponentModel.DataAnnotations;

namespace iGuruPrep.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        [Required(ErrorMessage = "Course name cannot be empty")]
        public string CourseName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Course code cannot be empty")]
        public string CourseCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        //public int? displayorder { get; set; }
        public int? EmployeeID { get; set;}
        //public string EmpFirstName { get; set; } = string.Empty;
    }
}