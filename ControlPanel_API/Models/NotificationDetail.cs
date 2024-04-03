namespace ControlPanel_API.Models
{
    public class NotificationDetail
    {
        public int ND_id { get; set; }
        public int? NBNotificationID { get; set; }
        public string NotificationDetails { get; set; } = string.Empty;
    }
}
