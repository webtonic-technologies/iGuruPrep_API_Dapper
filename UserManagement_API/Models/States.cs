namespace UserManagement_API.Models
{
    public class States
    {
        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
    public class Districts
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int StateID { get; set; }
        public bool Status { get; set; }
    }
}
