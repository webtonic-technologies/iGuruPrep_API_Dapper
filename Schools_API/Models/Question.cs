namespace Schools_API.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public string QuestionFormula { get; set; } = string.Empty;
        public string QuestionImage { get; set; } = string.Empty;
        public byte? DifficultyLevelId { get; set; }
        public byte? QuestionTypeId { get; set; }
        public int? SubjectIndexId { get; set; }
        public int? Duration { get; set; } = 1;
        public int? Occurrence { get; set; } = 1;
        public int? ComprehensiveId { get; set; } = 1;
        public int? ApprovedStatus { get; set; } = 1;
        public int? ApprovedBy { get; set; } = 1;
        public string ReasonNote { get; set; } = string.Empty;
        public int? ActualOption { get; set; } = 1;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? Verified { get; set; } = 1;

    }
}

