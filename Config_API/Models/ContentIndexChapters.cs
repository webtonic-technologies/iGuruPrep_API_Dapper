﻿namespace Config_API.Models
{
    public class ContentIndexChapters
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
    }
}
