namespace StudentApp_API.DTOs.Requests
{
    public class BoardPaperRequest
    {
    }
    public class TestSeriesQuestionRequest
    {
        public int RegistrationId {  get; set; }
        public int TestSeriesId { get; set; }
        public int SubjectId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<int>? QuestionTypeId { get; set; }
        public List<int>? QuestionStatus { get; set; }
    }
}
