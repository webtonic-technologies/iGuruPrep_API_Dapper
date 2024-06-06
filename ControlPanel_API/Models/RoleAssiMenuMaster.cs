namespace ControlPanel_API.Models
{
    public class RoleAssiMenuMaster
    {
        public int MenuMasterId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int ParentId { get; set; }
    }
}
