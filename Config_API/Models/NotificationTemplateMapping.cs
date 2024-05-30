namespace Config_API.Models
{
    public class NotificationTemplateMapping
    {
        public int TemplateMappingId { get; set; }
        public int NotificationTemplateID { get; set; }
        public string PlatformID { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool? isStudent { get; set; }
        public bool isEmployee { get; set; }
    }
}
