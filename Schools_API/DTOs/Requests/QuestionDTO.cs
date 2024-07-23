using Schools_API.Models;

namespace Schools_API.DTOs.Requests
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        //public string QuestionFormula { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        //public int? ApprovedStatus { get; set; }
        //public int? ApprovedBy { get; set; }
        //public string ReasonNote { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        //public int? Verified { get; set; }
        //public int? courseid { get; set; }
        //public int? boardid { get; set; }
        //public int? classid { get; set; }
        public int subjectID { get; set; }
       // public int ExamTypeId {  get; set; }
        public int EmployeeId { get; set; }
        //public string Rejectedby { get; set; } = string.Empty;
        //public DateTime RejectedDate {  get; set; }
        //public string RejectedReason { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation {  get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<QIDCourse>? QIDCourses { get; set; }
        public List<QuestionSubjectMapping>? QuestionSubjectMappings { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
        //public Reference? References { get; set; }
    }
    public class GetAllQuestionListRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int EmployeeId {  get; set; }
    }
    public class QuestionCompareRequest
    {
        public string NewQuestion { get; set; } = string.Empty;
    }
}
