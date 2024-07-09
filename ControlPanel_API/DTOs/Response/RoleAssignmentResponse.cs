namespace ControlPanel_API.DTOs.Response
{
    //public class RoleAssignmentResponse
    //{
    //    public int Employeeid { get; set; }
    //    public int Roleid { get; set; }
    //    public string Rolename { get; set; } = string.Empty;
    //    public int DesignationId { get; set; }
    //    public string DesignationName { get; set; } = string.Empty;
    //    public List<RoleAssignmentMappingResponse>? RoleAssignmentMappings { get; set; }
    //}
    //public class RoleAssignmentMappingResponse
    //{
    //    public int RAMappingId { get; set; }
    //    public int MenuMasterId { get; set; }
    //    public string MenuMasterName { get; set; } = string.Empty;
    //}
    public class RoleAssignmentResponse
    {
        public int RoleAssID { get; set; }
        public int RoleID { get; set; }
        public string Rolename { get; set; } = string.Empty;
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public List<ModuleSelectionResponse> ModuleSelection { get; set; } = new List<ModuleSelectionResponse>();
    }

    public class ModuleSelectionResponse
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public bool Status {  get; set; }
        public List<ModuleSubmoduleResponse> ModuleSubmodule { get; set; } = new List<ModuleSubmoduleResponse>();
    }

    public class ModuleSubmoduleResponse
    {
        public int ModuleSubID { get; set; }
        public int SubModuleId { get; set; }
        public string SubModuleName { get; set; } = string.Empty;
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}