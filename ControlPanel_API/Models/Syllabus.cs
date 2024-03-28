using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.Models
{
    public class Syllabus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SyllabusId { get; set; }
        public int? BoardID { get; set; }
        public int? CourseId { get; set; }
        public int? ClassId { get; set; }
        public string SyllabusName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public byte? YearID { get; set; } = 3;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
