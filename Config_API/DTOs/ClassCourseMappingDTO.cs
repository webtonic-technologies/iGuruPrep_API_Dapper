using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.DTOs
{
    public class ClassCourseMappingDTO
    {
        public int CourseClassMappingID { get; set; }
        public List<int> CourseID { get; set; }
        public int? ClassID { get; set; }
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
    }
}
