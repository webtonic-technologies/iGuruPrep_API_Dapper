using System.ComponentModel.DataAnnotations;
namespace Config_API.Models
{
    public class NotificationTemplateMapping
    {
        public int TemplateMappingId { get; set; }
        public int NotificationTemplateID { get; set; }
        [Required(ErrorMessage = "Platform name cannot be empty")]
        public string PlatformID { get; set; } = string.Empty;
        [Required(ErrorMessage = "Message cannot be empty")]
        public string Message { get; set; } = string.Empty;
        public bool? isStudent { get; set; } = false;
        public bool? isEmployee { get; set; } = false;
    }
}
