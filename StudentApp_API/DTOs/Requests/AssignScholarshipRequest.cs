namespace StudentApp_API.DTOs.Requests
{
    public class AssignScholarshipRequest
    {
        public int RegistrationID { get; set; }
    }
    public class ScholarshipQuestionSaveRequest
    {
        public int StudentId { get; set; } // Equivalent to RegistrationId in the original API
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
    }

}
