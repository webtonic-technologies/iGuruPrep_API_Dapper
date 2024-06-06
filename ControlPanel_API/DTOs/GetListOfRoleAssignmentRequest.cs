namespace ControlPanel_API.DTOs
{
    public class GetListOfRoleAssignmentRequest
    {
        public int RoleId { get; set; }
        public int DesignationId { get; set; }
    }
    public class MenuMasterDTOResponse
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public List<MenuMasterChild>? MenuMasterChildren { get; set; }
    }
    public class MenuMasterChild
    {
        public int ChildId { get; set; }
        public string ChildName { get; set; } = string.Empty;
    }
}
