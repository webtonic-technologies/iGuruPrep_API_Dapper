namespace Config_API.Models
{
    public class TypeOfTestSeries
    {
        public int TTSId { get; set; }
        public string TestSeriesName { get; set; } = string.Empty;
        public string TestSeriesCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}
