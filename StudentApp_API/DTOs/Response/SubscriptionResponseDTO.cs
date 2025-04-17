namespace StudentApp_API.DTOs.Response
{
    public class SubscriptionResponseDTO
    {
        public int SubscriptionID { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int ValidityDays { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public List<SubjectDetailsDTO>? Subjects { get; set; }
        public List<ModuleDTO>? Modules { get; set; }
    }

    public class SubjectDetailsDTO
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int ChapterCount { get; set; }
        public int ConceptCount { get; set; }
    }
    public class ModuleDTO
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public int ModuleTypeID { get; set; }
        public int? ParentModuleID { get; set; }
        public bool IsFree { get; set; }
        public bool IsSubscription { get; set; }
        public decimal DiscountOnFinalPrice { get; set; }
    }

    public class tblModulewiseConfiguration
    {
        public int MWCID { get; set; }
        public int ModuleID { get; set; }
        public bool IsFree { get; set; }
        public bool IsSubscription { get; set; }
        public decimal DiscountOnFinalPrice { get; set; }
    }

}
