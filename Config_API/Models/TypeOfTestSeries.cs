using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class TypeOfTestSeries
    {
        public int TTSId { get; set; }
        [Required(ErrorMessage = "Test series type name cannot be empty")]
        public string TestSeriesName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Test series type code cannot be empty")]
        public string TestSeriesCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
      //  public string EmpFirstName { get; set; } = string.Empty;
    }
}
