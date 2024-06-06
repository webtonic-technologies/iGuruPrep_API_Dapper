namespace Course_API.Models
{
    public class TestSeriesQuestionType
    {
        public int TestSeriesQuestionTypeId { get; set; }
        public int QuestionTypeID { get; set; }
        public int TestSeriesID { get; set; }
        public decimal EntermarksperCorrectAnswer { get; set; }
        public decimal EnterNegativeMarks { get; set; }
        public int PerNoofQuestions { get; set; }
        public int NoofQuestionsforChoice { get; set; }
    }
}
