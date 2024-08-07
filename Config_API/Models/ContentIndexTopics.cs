﻿namespace Config_API.Models
{
    public class ContentIndexTopics
    {
        public int ContInIdTopic { get; set; }
        public int ContentIndexId { get; set; }
        public string ContentName_Topic { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<ContentIndexSubTopic>? ContentIndexSubTopics { get; set; }
    }
    public class ContentIndexTopicsdto
    {
        public int ContInIdTopic { get; set; }
        public int ContentIndexId { get; set; }
        public string ContentName_Topic { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string ChapterCode {  get; set; } = string.Empty;
    }
}
