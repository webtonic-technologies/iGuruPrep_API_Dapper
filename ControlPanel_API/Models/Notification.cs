namespace ControlPanel_API.Models
{
    public class Notification
    {
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        public string NotificationDetails { get; set; } = string.Empty;
        public string NotificationLink { get; set; } = string.Empty;
        public int CourseID { get; set; }
        public string PathURL { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public int BoardId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
}
