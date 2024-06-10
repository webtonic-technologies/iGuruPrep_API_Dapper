using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.Models
{
    public class NotificationDetail
    {
        public int NBNotificationDetailid { get; set; }
        public int? NBNID { get; set; }
        [Required(ErrorMessage = "Detail cannot be empty")]
        public string NBNotificationDetail { get; set; } = string.Empty;
    }
}
