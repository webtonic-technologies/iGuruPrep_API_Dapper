using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class ClassCourseMapping
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseClassMappingID { get; set; }
        public int CourseID { get; set; }
        public int? ClassID { get; set; }
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;

        public string EmpFirstName { get; set; } = string.Empty;

        public string classname { get; set; } = string.Empty;

        public string coursename { get; set; } = string.Empty;

       

    }
}