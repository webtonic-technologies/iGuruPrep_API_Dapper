using Course_API.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel.DataAnnotations;

namespace Course_API.DTOs.Requests
{
    public class ScholarshipTestRequestDTO
    {
        public int ScholarshipTestId { get; set; }
        public int APID { get; set; }
        public int ExamTypeId { get; set; }
        public string PatternName { get; set; } = string.Empty;
        public int TotalNumberOfQuestions { get; set; }
        public string Duration { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public List<ScholarshipBoards>? ScholarshipBoards { get; set; }
        public List<ScholarshipClass>? ScholarshipClasses { get; set; }
        public List<ScholarshipCourse>? ScholarshipCourses { get; set; }
        public List<ScholarshipSubjects>? ScholarshipSubjects { get; set; }
    }
    public class ScholarshipBoards
    {
        public int SSTBoardId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int BoardId { get; set; }
    }
    public class ScholarshipClass
    {
        public int SSTClassId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int ClassId { get; set; }
    }
    public class ScholarshipCourse
    {
        public int SSTCourseId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int CourseId { get; set; }
    }
    public class ScholarshipSubjects
    {
        public int SSTSubjectId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int SubjectId { get; set; }
    }
    public class ScholarshipContentIndex
    {
        public int SSTContIndId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int SubjectId { get; set; }
    }
    public class ScholarshipQuestionSection
    {
        public int SSTSectionId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int QuestionTypeId { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal MarksPerQuestion { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal NegativeMarks { get; set; }
        public int TotalNumberOfQuestions { get; set; }
        public int NoOfQuestionsPerChoice { get; set; }
        public int SubjectId { get; set; }
        public List<ScholarshipSectionQuestionDifficulty>? ScholarshipSectionQuestionDifficulties { get; set; }
    }
    public class ScholarshipSectionQuestionDifficulty
    {
        public int Id { get; set; }
        public int SSTSectionId { get; set; }
        public int DifficultyLevelId { get; set; }
        public int QuesPerDiffiLevel { get; set; }
    }
    public class ScholarshipTestInstructions
    {
        public int SSTInstructionsId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string InstructionName { get; set; } = string.Empty;
        public int InstructionId { get; set; }
        public int ScholarshipTestId { get; set; }
    }
    public class ScholarshipTestDiscountScheme
    {
        public int SSTDiscountSchemeId { get; set; }
        public int ScholarshipTestId { get; set; }
        public string PercentageStartRange { get; set; } = string.Empty;
        public string PercentageEndRange { get; set; } = string.Empty;
        public string Discount { get; set; } = string.Empty;
    }
    public class ScholarshipTestQuestion
    {
        public int SSTQuestionsId {  get; set; }
        public int ScholarshipTestId { get; set; }
		public int SubjectId { get; set; }
		public int DisplayOrder { get; set; }
		public int SSTSectionId { get; set; }
		public int QuestionId { get; set; }
		public string QuestionCode { get; set; } = string.Empty;
    }
    public class ScholarshipGetListRequest
    {
        public int APId { get; set; }
        public int BoardId {  get; set; }
        public int ClassId {  get; set; }
        public int CourseId {  get; set; }
        public int ExamTypeId {  get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
    public class QuestionSectionScholarship
    {
        public int SubjectId { get; set; }
        public List<ScholarshipQuestionSection>? ScholarshipQuestionSections { get; set; }
    }
}