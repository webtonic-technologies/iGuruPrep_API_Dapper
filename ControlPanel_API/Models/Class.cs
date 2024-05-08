namespace ControlPanel_API.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public bool? showcourse { get; set; }
        public int? EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}
