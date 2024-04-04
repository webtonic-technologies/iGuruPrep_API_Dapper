namespace Course_API.Models
{
    public class Syllabus
    {
        public int SyllabusId { get; set; }
        public int BoardID { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? YearID { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? SubjectId { get; set; }
    }
}
