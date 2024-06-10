using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    public class Designation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]      
        public int DesgnID { get; set; }
        [Required(ErrorMessage = "Designation name cannot be empty")]
        public string DesignationName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Designation code cannot be empty")]
        public string DesgnCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? createdon { get; set; }
        public DateTime? modifiedon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
    }
}