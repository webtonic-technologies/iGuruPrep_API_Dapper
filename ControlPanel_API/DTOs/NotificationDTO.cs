using ControlPanel_API.Models;
using System.Diagnostics.CodeAnalysis;

namespace ControlPanel_API.DTOs
{
    public class NotificationDTO
    {
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        public string NotificationDetails { get; set; } = string.Empty;
        public string NotificationLink { get; set; } = string.Empty;
        public int CourseID { get; set; }
        public IFormFile? PathURL { get; set; }
        public int ClassID { get; set; }
        public int BoardId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public List<NotificationLinkMaster>? NotificationLinkMasters { get; set; }
        public List<NotificationDetail>? NotificationDetailMasters { get; set; }
    }
    public class NotificationImageDTO
    {
        public int NBNotificationID { get; set; }
        public IFormFile? PathURL { get; set; }
    }
}
