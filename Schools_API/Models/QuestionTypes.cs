namespace Schools_API.Models
{
    public class QuestionTypes
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string AnswerCategory { get; set; } = string.Empty;
        public string AnswerCode { get; set; } = string.Empty;
    }
}
