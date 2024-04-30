namespace Config_API.Models
{
    public class Module
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public int? ModuleTypeID { get; set; }
        public int? ParentModuleID { get; set; }
        public bool Status { get; set; }
    }
}