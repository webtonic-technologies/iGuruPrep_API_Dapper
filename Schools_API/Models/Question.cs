namespace Schools_API.Models
{
    public class Question
    {

        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        //public string QuestionFormula { get; set; } = string.Empty;
        // public string QuestionImage { get; set; } = string.Empty;
        //public int DifficultyLevelId { get; set; }
        public int QuestionTypeId { get; set; }
        // public int? SubjectIndexId { get; set; }
        //public int? Duration { get; set; }
        //public int? Occurrence { get; set; }
        //public int? ApprovedStatus { get; set; }
        //public int? ApprovedBy { get; set; }
        //public string ReasonNote { get; set; } = string.Empty;
        //public int? ActualOption { get; set; }
        public bool? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        //public int? Verified { get; set; }
        //public int? courseid { get; set; }
        //public int? boardid { get; set; }
        //public int? classid { get; set; }
        public int subjectID { get; set; }
        //public int ExamTypeId { get; set; }
        public int EmployeeId { get; set; }
        public int CategoryId {  get; set; }
        public int ModifierId { get; set; }
        //public int userid { get; set; }
        //public string Rejectedby { get; set; } = string.Empty;
        //public string RejectedReason { get; set; } = string.Empty;
        //public string APName { get; set; } = string.Empty;
        //public string BoardName { get; set; } = string.Empty;
        //public string ClassName { get; set; } = string.Empty;
        //public string CourseName { get; set; } = string.Empty;
        //public string SubjectName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public bool? IsRejected { get; set; }
        public bool? IsApproved { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
    }
}