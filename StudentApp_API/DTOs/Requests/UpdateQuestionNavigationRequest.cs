namespace StudentApp_API.DTOs.Requests
{
    public class UpdateQuestionNavigationRequest
    {
        public int ScholarshipID { get; set; }
        public int RegistrationId { get; set; }
        public int QuestionID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
