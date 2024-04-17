namespace Config_API.Models
{
    public class NotificationTemplate
    {
        public int NotificationTemplateID { get; set; }
        public string Notification { get; set; } = string.Empty;
        public int? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? platformid { get; set; }
        public int? moduleid { get; set; }
    }
}
