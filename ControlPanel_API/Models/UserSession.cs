namespace ControlPanel_API.Models
{
    public class UserSession
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }
        public bool IsActive { get; set; }
    }
}
