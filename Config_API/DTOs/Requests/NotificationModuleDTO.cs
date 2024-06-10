using Config_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Config_API.DTOs.Requests
{
    public class NotificationDTO
    {
        public int NotificationTemplateID { get; set; }
        public bool Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        [Required(ErrorMessage = "Module cannot be empty")]
        public int? moduleID { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        // public string EmpFirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Sub module cannot be empty")]
        public int subModuleId { get; set; }
        public List<NotificationTemplateMapping>? NotificationTemplateMappings { get; set; }
    }
    public class GetAllNotificationModRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
