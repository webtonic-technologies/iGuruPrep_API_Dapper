using System.ComponentModel.DataAnnotations;

namespace iGuruPrep.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        [Required(ErrorMessage = "Class name cannot be empty")]
        public string ClassName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Class code cannot be empty")]
        public string ClassCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
       // public bool? showcourse { get; set; }
        public int? EmployeeID { get; set; }
       // public string EmpFirstName { get; set; } = string.Empty;
    }
}