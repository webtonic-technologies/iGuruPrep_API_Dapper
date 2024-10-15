namespace Course_API.Models
{
    public class TestSeriesQuestions
    {
        public int testseriesquestionsid { get; set; }
        public int TestSeriesid { get; set; }
        public int Questionid { get; set; }
        public int DisplayOrder { get; set; }
        public int Status { get; set; }
        public int testseriesQuestionSectionid { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string QuestionDescription {  get; set; } = string.Empty;
    }
}
//sub id