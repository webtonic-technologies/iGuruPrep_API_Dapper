using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class StatusMessages
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusId { get; set; }
        public int? StatusCode { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        public string EmpFirstName { get; set; }
        
    }
}