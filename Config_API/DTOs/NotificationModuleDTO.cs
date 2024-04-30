namespace Config_API.DTOs
{
    public class NotificationModuleDTO
    {
        public string ModuleName { get; set; } = string.Empty;
        public List<NotificationDTO>? NotificationDTOs { get; set; }
    }
    public class NotificationDTO
    {
        public int NotificationTemplateID { get; set; }
        public string Platformname { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
