namespace StudentApp_API.DTOs.Requests
{
    public class GetScholarshipTestRequest
    {
        public int RegistrationId { get; set; }
        public int ScholarshipID { get; set; }
    }
    public class AnswerSubmissionRequest
    {
        public int ScholarshipID { get; set; }
        public int RegistrationId { get; set; }
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }
        public int QuestionTypeID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime {  get; set; }
        public List<int>? AnswerID { get; set; }
    }
    public class QuestionAnswerData
    {
        public int QuestionId { get; set; }
        public int QuestionTypeId { get; set; }
        public int CorrectAnswerId { get; set; }
        public decimal MarksPerQuestion { get; set; }
        public decimal NegativeMarks { get; set; }
    }
}
