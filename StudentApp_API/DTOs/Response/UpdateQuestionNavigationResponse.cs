namespace StudentApp_API.DTOs.Responses
{
    public class UpdateQuestionNavigationResponse
    {
        public int NavigationID { get; set; }
        public int ScholarshipID { get; set; }
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Message { get; set; }
    }
}
