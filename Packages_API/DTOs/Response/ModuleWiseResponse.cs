namespace Packages_API.DTOs.Response
{
    public class ModuleWiseResponse
    {
    }
    public class ModuleDTO
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
    }
    public class ModuleWiseConfigDTO
    {
        public int MWCID { get; set; }
        public int ModuleID { get; set; }
        public bool IsFree { get; set; }
        public bool IsSubscription { get; set; }
        public decimal DiscountOnFinalPrice { get; set; }
    }

}
