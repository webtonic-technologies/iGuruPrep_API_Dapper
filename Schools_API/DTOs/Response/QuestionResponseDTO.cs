using Schools_API.Models;

namespace Schools_API.DTOs.Response
{
    public class QuestionResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public bool? IsRejected { get; set; }
        public bool? IsApproved { get; set; }
        public string QuestionTypeName {  get; set; } = string.Empty;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string userRole {  get; set; } = string.Empty;
        public List<QIDCourseResponse>? QIDCourses { get; set; }
        public List<QuestionSubjectMappingResponse>? QuestionSubjectMappings { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }
    public class QIDCourseResponse
    {
        public int QIDCourseID { get; set; }
        public int? QID { get; set; }
        public int? CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int? LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
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
        public string QuestionCode { get; set; } = string.Empty;
    }
}
