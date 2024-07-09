namespace ControlPanel_API.Models
{
    public class RoleAssignmentMapping
    {
        public int RAMappingId { get; set; }
        public int MenuMasterId { get; set; }
        public int DesignationId { get; set; }
        public int RoleId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public bool Status {  get; set; }
    }
}
