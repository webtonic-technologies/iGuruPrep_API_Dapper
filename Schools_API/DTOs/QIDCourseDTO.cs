namespace Schools_API.Models
{
    public class QIDCourseDTO
    {
        public int QIDCourseID { get; set; }
        public int QID { get; set; }
        public int CourseID { get; set; }
        public int LevelId { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
