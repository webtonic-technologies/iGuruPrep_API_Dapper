namespace ControlPanel_API.DTOs.Response
{
    public class RoleAssignmentResponse
    {
        public int Employeeid { get; set; }
        public List<RoleAssignmentMappingResponse>? RoleAssignmentMappings { get; set; }
    }
    public class RoleAssignmentMappingResponse
    {
        public int RAMappingId { get; set; }
        public int MenuMasterId { get; set; }
        public string MenuMasterName { get; set; } = string.Empty;
    }
}