namespace ControlPanel_API.Models
{
    public class NotificationLinkMaster
    {
        public int NL_id { get; set; }
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        public string NotificationLink { get; set; } = string.Empty;
    }
}
