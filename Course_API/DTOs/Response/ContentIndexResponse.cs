namespace Course_API.DTOs.Response
{
    public class ContentIndexResponse
    {
        public int ContentIndexId { get; set; }
        public int? SubjectId { get; set; }
        public string? ContentName_Chapter { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? IndexTypeId { get; set; }
        //public int BoardId { get; set; }
        //public int ClassId { get; set; }
        //public int CourseId { get; set; }
        public int? APID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        //public int ExamTypeId { get; set; }
        public string? ChapterCode { get; set; } = string.Empty;
        public List<ContentIndexTopics>? ContentIndexTopics { get; set; }
    }
    public class ContentIndexTopics
    {
        public int ContInIdTopic { get; set; }
        public int ContentIndexId { get; set; }
        public string? ContentName_Topic { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int? EmployeeId { get; set; }
        public string? ChapterCode { get; set; } = string.Empty;
        public string? TopicCode {  get; set; } = string.Empty;
        public List<ContentIndexSubTopic>? ContentIndexSubTopics { get; set; }
    }
    public class ContentIndexSubTopic
    {
        public int? ContInIdSubTopic { get; set; }
        public int? ContInIdTopic { get; set; }
        public string? ContentName_SubTopic { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public string? TopicCode { get; set; } = string.Empty;
        public string? SubTopicCode { get; set; } = string.Empty;
    }
    // Define classes for the response structure
    public class SyllabusDetail
    {
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int SubjectId { get; set; }
    }
    public class ContentIndexResponses
    {
        public int? ContentIndexId { get; set; }
        public int? SubjectId { get; set; }
        public string ContentName_Chapter { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? IndexTypeId { get; set; }
        public int? BoardId { get; set; }
        public int? ClassId { get; set; }
        public int? CourseId { get; set; }
        public int? APID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public int? ExamTypeId { get; set; }
        public bool? IsActive { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int? DisplayOrder { get; set; }
        public string QuestionCountPerDifficultyLevel {  get; set; } = string.Empty;
        public List<ContentIndexTopicsResponse>? ContentIndexTopics { get; set; }
    }
    public class ContentIndexTopicsResponse
    {
        public int ContInIdTopic { get; set; }
        public int? ContentIndexId { get; set; }
        public string ContentName_Topic { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int? EmployeeId { get; set; }
        public bool? IsActive { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int? DisplayOrder { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string QuestionCountPerDifficultyLevel { get; set; } = string.Empty;
        public List<ContentIndexSubTopicResponse>? ContentIndexSubTopics { get; set; }
    }
    public class ContentIndexSubTopicResponse
    {
        public int? ContInIdSubTopic { get; set; }
        public int? ContInIdTopic { get; set; }
        public string ContentName_SubTopic { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int? IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public bool? IsActive { get; set; }
        public string SubTopicCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int? DisplayOrder { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string QuestionCountPerDifficultyLevel { get; set; } = string.Empty;
    }
}
