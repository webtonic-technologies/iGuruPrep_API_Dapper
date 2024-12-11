using Schools_API.Models;

namespace Schools_API.DTOs.Requests
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        public int EmployeeId { get; set; }
        public int ModifierId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<QIDCourse>? QIDCourses { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }
    public class GetAllQuestionListRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int EmployeeId { get; set; }
    }
    public class QuestionCompareRequest
    {
        public string NewQuestion { get; set; } = string.Empty;
    }
    public class UpdateQIDCourseRequest
    {
        public int QID { get; set; }
        public int CourseID { get; set; }
        public int LevelId { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public string QuestionCode { get; set; }
    }
    public class ComprehensiveQuestionRequest
    {
        public int QuestionId { get; set; }
        public string Paragraph { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        public int EmployeeId { get; set; }
        public int ModifierId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<QIDCourse>? QIDCourses { get; set; }
        public List<ParagraphQuestionDTO>? Questions { get; set; }
    }
    public class ParagraphQuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        // public int ParentQId {  get; set; }
        // public string ParentQCode { get; set; }
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        //  public int CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        // public int subjectID { get; set; }
        // public int EmployeeId { get; set; }
        // public int ModifierId { get; set; }
        // public int IndexTypeId { get; set; }
        //  public int ContentIndexId { get; set; }
        //  public bool? IsRejected { get; set; } = false;
        //  public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string? Explanation { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }
    public class ParagraphChildQuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        // public int ParentQId {  get; set; }
        // public string ParentQCode { get; set; }
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        // public int EmployeeId { get; set; }
        // public int ModifierId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        //  public bool? IsRejected { get; set; } = false;
        //  public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string? Explanation { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }

    public class MatchThePairRequest
    {
        public int QuestionId { get; set; }
        public int QuestionTypeId { get; set; }
        public string QuestionCode { get; set; }
        public int Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int EmployeeId { get; set; }
        public int ModifierId { get; set; }
        public int SubjectID { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int ExamTypeId { get; set; }
        public int CategoryId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string Explanation { get; set; }
        public string ExtraInformation { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<QIDCourse>? QIDCourses { get; set; }
        public List<MatchPair> MatchPairs { get; set; }
        public List<AnswerMultipleChoiceCategory> AnswerMultipleChoiceCategories { get; set; }
    }

    public class MatchPair
    {
        public int MatchThePairId {  get; set; }
        public int PairColumn { get; set; }
        public int PairRow { get; set; }
        public string PairValue { get; set; }
    }
    public class MatchThePair2Request
    {
        public int QuestionId { get; set; }
        public int QuestionTypeId { get; set; }
        public string QuestionCode { get; set; }
        public int Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int EmployeeId { get; set; }
        public int ModifierId { get; set; }
        public int SubjectID { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int ExamTypeId { get; set; }
        public int CategoryId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string Explanation { get; set; }
        public string ExtraInformation { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<QIDCourse>? QIDCourses { get; set; }
        public List<MatchPair> MatchPairs { get; set; }
        public List<MatchThePairAnswer> MatchThePairAnswers { get; set; }
    }
    public class MatchThePairAnswer
    {
        public int MatchThePair2Id {  get; set; }
        public int PairColumn { get; set; }
        public int PairRow { get; set; }
    }
}