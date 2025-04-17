namespace StudentApp_API.DTOs.Requests
{
    public class PreviousPapersRequest
    {
    }
    public class GetTestSeriesList
    {
        public int registrationId { get; set; }
        public int courseId {  get; set; }
        public bool? IsStarted {  get; set; }
    }
    public class SaveQuestionPreviousPaperRequest
    {
        public int QuestionId { get; set; }
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
        public int TestSeriesId { get; set; }
    }
}
