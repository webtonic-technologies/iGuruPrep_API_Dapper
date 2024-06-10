using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.Models
{
    public class HelpFAQ
    {
        public int HelpFAQId { get; set; }
        [Required(ErrorMessage = "Question name cannot be empty")]
        public string FAQName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Answer cannot be empty")]
        public string FAQAnswer { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}
