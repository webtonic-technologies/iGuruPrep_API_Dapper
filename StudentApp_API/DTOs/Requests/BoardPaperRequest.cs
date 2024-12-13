namespace StudentApp_API.DTOs.Requests
{
    public class BoardPaperRequest
    {
    }
    public class TestSeriesQuestionRequest
    {
        public int TestSeriesId { get; set; }
        public int SubjectId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<int>? QuestionTypeId { get; set; }
    }
}
