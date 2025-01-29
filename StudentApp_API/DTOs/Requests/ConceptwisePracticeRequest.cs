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
    public class ConceptwisePracticeSubmitAnswerRequest
    {
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public string AnswerID { get; set; }
        public DateTime? StaTime { get; set; }
        public DateTime? EndTime {  get; set; }
    }
    public class GetQuestionsList
    {
        public int subjectId { get; set; }
        public int indexTypeId { get; set; }
        public int contentId { get; set; }
        public int SyllabusId {  get; set; }
        public int CourseId {  get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }
    public class SaveQuestionConceptwisePracticeRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
    }
}
