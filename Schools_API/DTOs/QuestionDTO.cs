namespace Schools_API.Models
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public string QuestionFormula { get; set; } = string.Empty;
        public FormFile? QuestionImage { get; set; }
        public byte? DifficultyLevelId { get; set; }
        public byte? QuestionTypeId { get; set; }
        public int? SubjectIndexId { get; set; }
        // public int? Duration { get; set; }
        // public int? Occurrence { get; set; }
        //  public int? ComprehensiveId { get; set; }
        //  public int? ApprovedStatus { get; set; }
        //  public int? ApprovedBy { get; set; }
        //  public string ReasonNote { get; set; } = string.Empty;
        //  public int? ActualOption { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        //  public int? Verified { get; set; }
        public List<QIDCourseDTO>? QIDCourses { get; set; }
        public Reference? References { get; set; }
    }
    public class QuestionImageDTO
    {
        public int QuestionId { get; set; }
        public FormFile? QuestionImage { get; set; }
    }
}
