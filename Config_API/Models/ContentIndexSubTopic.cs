﻿namespace Config_API.Models
{
    public class ContentIndexSubTopic
    {
        public int ContInIdSubTopic { get; set; }
        public int ContInIdTopic { get; set; }
        public string ContentName_SubTopic { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int IndexTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string SubTopicCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder {  get; set; }
        public string TopicCode {  get; set; } = string.Empty;
    }
}
