using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int RoleId { get; set; }
        [Required]
        public int? RoleNumber { get; set; }
        [Required]
        public string RoleName { get; set; } = string.Empty;
        [Required]
        public string RoleCode { get; set; } = string.Empty;
        [Required]
        public bool? Status { get; set; }
        public DateTime? createdon {  get; set; }
        public DateTime? modifiedon {  get; set; }
        public string createdby { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
    }
}
