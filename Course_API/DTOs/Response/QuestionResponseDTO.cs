namespace Course_API.DTOs.Response
{
    public class QuestionResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public string QuestionFormula { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public int? ApprovedStatus { get; set; }
        public int? ApprovedBy { get; set; }
        public string ReasonNote { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? Verified { get; set; }
        public int? courseid { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int? boardid { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public int? classid { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int subjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Rejectedby { get; set; } = string.Empty;
        public string RejectedReason { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public List<QIDCourseResponse>? QIDCourses { get; set; }
        public List<QuestionSubjectMappingResponse>? QuestionSubjectMappings { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
        public Reference? References { get; set; }
    }
    public class QIDCourseResponse
    {
        public int QIDCourseID { get; set; }
        public int QID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class QuestionSubjectMappingResponse
    {
        public int QuestionSubjectid { get; set; }
        public int ContentIndexId { get; set; }
        public int Indexid { get; set; }
        public int questionid { get; set; }
        public int Levelid { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public string ContentIndexName { get; set; } = string.Empty;
        // public int SubjectIndexId { get; set; }
    }
    public class Reference
    {
        public int ReferenceId { get; set; }
        //public int? SubjectIndexId { get; set; }
        //public string Type { get; set; } = string.Empty;
        public string ReferenceNotes { get; set; } = string.Empty;
        public string ReferenceURL { get; set; } = string.Empty;
        public int? QuestionId { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
    public class AnswerMultipleChoiceCategory
    {
        public int Answermultiplechoicecategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
        public bool? Iscorrect { get; set; }
        public int? Matchid { get; set; }
    }
    public class Answersingleanswercategory
    {
        public int Answersingleanswercategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
