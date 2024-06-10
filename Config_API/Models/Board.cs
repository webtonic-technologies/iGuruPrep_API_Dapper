using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Board
{
    public int BoardId { get; set; }
    [Required(ErrorMessage = "Board name cannot be empty")]
    public string BoardName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Board code cannot be empty")]
    public string BoardCode { get; set; } = string.Empty;
    public bool? Status { get; set; }
    //public bool? showcourse { get; set; }
    public DateTime? createdon { get; set; }
    public string createdby { get; set; } = string.Empty;
    public DateTime? modifiedon { get; set; }
    public string modifiedby { get; set; } = string.Empty;
    public int? EmployeeID { get; set; }
    //public string EmpFirstName { get; set; } = string.Empty;
}
