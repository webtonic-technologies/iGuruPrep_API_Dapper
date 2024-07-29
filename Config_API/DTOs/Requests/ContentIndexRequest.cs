using Config_API.Models;

namespace Config_API.DTOs.Requests
{
    public class ContentIndexRequest
    {
        public int ContentIndexId { get; set; }
        public int SubjectId { get; set; }
        public string ContentName_Chapter { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int IndexTypeId { get; set; }
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int APID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public int ExamTypeId {  get; set; }
        public bool IsActive {  get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<ContentIndexTopics>? ContentIndexTopics { get; set; }
    }
    public class ContentIndexRequestdto
    {
        public int ContentIndexId { get; set; }
        public int SubjectId { get; set; }
        public string ContentName_Chapter { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int IndexTypeId { get; set; }
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int APID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public int ExamTypeId { get; set; }
        public bool IsActive { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
    public class ContentIndexSheet
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Chapter { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string SubTopic { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int DisplayOrderChapter { get; set; }
        public int DisplayOrderTopic {  get; set; }
        public int DisplayOrderSubTopic { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public string SubTopicCode { get; set; } = string.Empty;
    }
    public class ContentIndexEntry
    {
        public string SubjectCode { get; set; }
        public string Chapter { get; set; }
        public int DisplayOrderChapter { get; set; }
        public string Topic { get; set; }
        public int DisplayOrderTopic { get; set; }
        public string SubTopic { get; set; }
        public int DisplayOrderSubTopic { get; set; }
        public string ChapterCode { get; set; }
        public string TopicCode { get; set; }
        public string SubTopicCode { get; set; }
    }
}