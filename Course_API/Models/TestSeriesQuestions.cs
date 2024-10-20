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
        public int QuestionTypeId { get; set; }
        public List<AnswerMultipleChoiceCategorys>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategorys? Answersingleanswercategories { get; set; }
    }
    public class AnswerMultipleChoiceCategorys
    {
        public int Answermultiplechoicecategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
        public bool? Iscorrect { get; set; }
        public int? Matchid { get; set; }
    }
    public class Answersingleanswercategorys
    {
        public int Answersingleanswercategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
//sub id