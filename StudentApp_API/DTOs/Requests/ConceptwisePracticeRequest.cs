using System.Diagnostics.CodeAnalysis;

namespace StudentApp_API.DTOs.Requests
{
    public class ConceptwisePracticeRequest
    {
    }
    public class ConcepwiseAnswerSubmissionRequest
    {
        public int RegistrationId { get; set; }
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }
        public int QuestionTypeID { get; set; }
        public List<int> AnswerID { get; set; }
    }
    public class ChapterAccuracyReportRequest
    {
        public int StudentId { get; set; }
        public int SetId { get; set; }
        public int IndexTypeId { get; set; } = 1; // Fixed for chapter
        public int ContentId { get; set; }
    }

    public class ConceptwisePracticeSubmitAnswerRequest
    {
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }
        public int QuestionTypeID { get; set; }
        public int QuestionstatusId { get; set; }
        public DateTime? StaTime {  get; set; }
        public DateTime? EndTime {  get; set; }
        public List<int>? MultiOrSingleAnswerId { get; set; }
        public string? SubjectiveAnswers { get; set; } = string.Empty;
    }
    public class GetQuestionsList
    {
        public int subjectId { get; set; }
        public int indexTypeId { get; set; }
        public int StudentId {  get; set; }
        public int contentId { get; set; }
        public int SyllabusId {  get; set; }
        public int CourseId {  get; set; }
        public List<int>? DifficultyLevel {  get; set; }
        public List<int>? QuestionStatus {  get; set; }
    }
    public class SaveQuestionConceptwisePracticeRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
    }
}
