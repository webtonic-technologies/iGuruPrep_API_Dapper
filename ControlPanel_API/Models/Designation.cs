using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    public class Designation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]      
        public int DesgnID { get; set; }
        [Required]
        public string DesignationName { get; set; } = string.Empty;
        [Required]
        public string DesgnCode { get; set; } = string.Empty;
        [Required]
        public bool? Status { get; set; }
    }
}