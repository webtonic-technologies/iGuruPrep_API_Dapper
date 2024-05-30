using Config_API.Models;

namespace Config_API.DTOs
{
    public class NotificationDTO
    {
        public int NotificationTemplateID { get; set; }
        public int? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public int? moduleID { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public int subModuleId { get; set; }
        public List<NotificationTemplateMapping>? NotificationTemplateMappings { get; set; }
    }

    public class NotificationResponseDTO
    {
        public int NotificationTemplateID { get; set; }
        public int? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public int? moduleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public int subModuleId { get; set; }
        public string SubModuleName { get; set; } = string.Empty;
        public List<NotificationTemplateMappingResponse>? NotificationTemplateMappings { get; set; }
    }

    public class NotificationTemplateMappingResponse
    {
        public int TemplateMappingId { get; set; }
        public int NotificationTemplateID { get; set; }
        public IEnumerable<Platform>? PlatformDatas { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool? isStudent { get; set; }
        public bool isEmployee { get; set; }
    }
}
