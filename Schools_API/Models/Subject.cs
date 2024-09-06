namespace Schools_API.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? DisplayOrder { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? SubjectType { get; set; }
    }
    public class Chapters
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
    }
    public class Topics
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
    }
    public class SubTopic
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
        public int DisplayOrder { get; set; }
        public string TopicCode { get; set; } = string.Empty;
    }
    public class DifficultyLevel
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string LevelCode { get; set; }
        public string Status { get; set; }
        public int NoofQperLevel { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime createdon { get; set; }
        public string patterncode { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; }
        public string createdby { get; set; }
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; }
    }
    public class Questiontype
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int MinNoOfOptions { get; set; }
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public int TypeOfOption { get; set; }
    }
}
