namespace Schools_API.DTOs.Requests
{
    public class ReportedQuestionQueryRequest
    {
        public int QueryCode { get; set; }
        public int QuestionID { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
    }
    public class ReportedQuestionRequest
    {
        public int? SubjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
    }
}