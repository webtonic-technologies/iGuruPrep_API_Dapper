using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class SyllabusSubject
    {
        public int SyllabusSubjectID { get; set; }
        public int SyllabusID { get; set; }
        [Required(ErrorMessage = "Subject name cannot be empty")]
        public int? SubjectID { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
