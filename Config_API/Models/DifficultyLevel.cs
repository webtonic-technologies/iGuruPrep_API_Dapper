using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class DifficultyLevel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte LevelId { get; set; }
        [Required(ErrorMessage = "Level name cannot be empty")]
        public string LevelName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Level code cannot be empty")]
        public string LevelCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? NoofQperLevel {  get; set; }
        public decimal? SuccessRate { get; set; }
        public DateTime? createdon { get; set; }
       // public string patterncode { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public string createdby { get; set; } = string.Empty;
        public int? EmployeeID {  get; set; }
       // public string EmpFirstName { get; set; } = string.Empty;
    }
}