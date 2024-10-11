using Course_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Course_API.DTOs.Requests
{
    public class TestSeriesDTO
    {
        public int TestSeriesId { get; set; }
        //public int boardid { get; set; }
        //public int classId { get; set; }
        //public int CourseId { get; set; }
        public DateTime? createdon { get; set; }
        public string? createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string? modifiedby { get; set; } = string.Empty;
        [Required(ErrorMessage = "Pattern name cannot be empty")]
        public string TestPatternName { get; set; } = string.Empty;
        //public string BoardName { get; set; } = string.Empty;
        //public string ClassName { get; set; } = string.Empty;
        //public string CourseName { get; set; } = string.Empty;
        //public string ExamTypeName { get; set; } = string.Empty;
        //public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Duration cannot be empty")]
        public string? Duration { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int APID { get; set; }
        // public string APName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Total number of questions cannot be empty")]
        public int? TotalNoOfQuestions { get; set; }
        public bool? MethodofAddingType { get; set; }
        [Required(ErrorMessage = "Start Date cannot be empty")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Start time cannot be empty")]
        public string? StartTime { get; set; } = string.Empty;
        [Required(ErrorMessage = "Result date cannot be empty")]
        public DateTime? ResultDate { get; set; }
        [Required(ErrorMessage = "Result time cannot be empty")]
        public string? ResultTime { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Exam name cannot be empty")]
        public string? NameOfExam { get; set; } = string.Empty;
        public bool? RepeatedExams { get; set; }
        public int? TypeOfTestSeries { get; set; }
        public int? ExamTypeID { get; set; }
        public List<TestSeriesBoards>? TestSeriesBoard { get; set; }
        public List<TestSeriesClass>? TestSeriesClasses { get; set; }
        public List<TestSeriesCourse>? TestSeriesCourses { get; set; }
        public List<TestSeriesSubjects>? TestSeriesSubject { get; set; }
        // public List<TestSeriesContentIndex>? TestSeriesContentIndexes { get; set; }
        // public TestSeriesQuestionSection? TestSeriesQuestionsSection { get; set; }
        // public List<TestSeriesQuestionType>? TestSeriesQuestionTypes { get; set; }
        // public List<TestSeriesInstructions>? TestSeriesInstruction { get; set; }
        // public List<TestSeriesQuestions>? TestSeriesQuestions { get; set; }
        public DateTime? RepeatExamStartDate { get; set; }
        public DateTime? RepeatExamEndDate { get; set; }
        public string? RepeatExamStarttime { get; set; } = string.Empty;
        public int? RepeatExamResulttimeId { get; set; }
        public bool IsAdmin {  get; set; }
        public bool? IsMandatory { get; set; }
    }
    public class GetAllQuestionListRequest
    {
        public int Subjectid { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentId { get; set; }
        public int SectionId { get; set; }
        public int QuestionTypeId { get; set; }
        public int DifficultyLevelId { get; set; }
        public int PageNumber {  get; set; }
        public int PageSize {  get; set; }
    }
    public class SyllabusDetailsRequest
    {
        public int TestSeriesId { get; set; }
        public int SubjectId { get; set; }
    }
    public class QuestionListRequest
    {
        public int TestSeriesId { get; set; }
        public int SubjectId { get; set; }
        public int SectionId { get; set; }
    }
    public class TestSeriesListRequest
    {
        public int APId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int BoardId { get; set; }
        public int ExamTypeId { get; set; }
        public int TypeOfTestSeries { get; set; }
        public string? ExamStatus { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public int PageNumber {  get; set; }
        public int PageSize {  get; set; }
        public bool IsAdmin {  get; set; }
    }

    public class ContentIndexRequest
    {
        public int TestSeriesID { get; set; }
        public List<Subject> Subjects { get; set; }
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public List<Chapter> Chapter { get; set; }
    }

    public class Chapter
    {
        public int TestseriesContentIndexId { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public bool Status { get; set; }
        public List<Concept> Concepts { get; set; }
    }

    public class Concept
    {
        public int TestseriesConceptIndexId { get; set; }
        public int ContInIdTopic { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public bool Status { get; set; }
        public List<SubConcept> SubConcepts { get; set; }
    }

    public class SubConcept
    {
        public int TestseriesConceptIndexId { get; set; }
        public int ContInIdSubTopic { get; set; }
        public int ContInIdTopic { get; set; }
        public int IndexTypeId { get; set; }
        public bool Status { get; set; }
    }
    public class TestseriesProfilerRequest
    {
        public int TestSeriesId { get; set; }
        public int EmployeeId {  get; set; }
    }
    public class DownExcelRequest
    {
        public int subjectId { get; set; }
        public int indexTypeId { get; set; } //1 means chapter, 2 means topic and 3 means sub topic
        public int contentId { get; set; }
    }
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        public int EmployeeId { get; set; }
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
    public class QIDCourse
    {
        public int QIDCourseID { get; set; }
        public int QID { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int CourseID { get; set; }
        public int LevelId { get; set; }
        public bool? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
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
