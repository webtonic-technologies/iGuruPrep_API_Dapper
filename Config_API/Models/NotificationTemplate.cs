namespace Config_API.Models
{
    public class NotificationTemplate
    {
        public int NotificationTemplateID { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public int? PlatformID { get; set; }
        public int? moduleID { get; set; }
        public DateTime? modifiedon {  get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        //EmpFirstName
    }
}