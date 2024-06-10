using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int RoleId { get; set; }
        public int? RoleNumber { get; set; }
        [Required(ErrorMessage = "Role name cannot be empty")]
        public string RoleName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Role code cannot be empty")]
        public string RoleCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? createdon {  get; set; }
        public DateTime? modifiedon {  get; set; }
        public string createdby { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
    }
}
