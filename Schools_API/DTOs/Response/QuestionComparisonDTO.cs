namespace Schools_API.DTOs.Response
{
    public class QuestionComparisonDTO
    {
        public int QuestionID { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public double Similarity { get; set; }
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
        public int Count {  get; set; }
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
        public int Count { get; set; }
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
        public int Count { get; set; }
    }
}
