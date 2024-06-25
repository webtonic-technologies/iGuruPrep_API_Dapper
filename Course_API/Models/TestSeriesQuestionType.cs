using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class TestSeriesQuestionType
    {
        public int TestSeriesQuestionTypeId { get; set; }
        [Required(ErrorMessage = "Question type cannot be empty")]
        public int QuestionTypeID { get; set; }
        public int TestSeriesID { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EntermarksperCorrectAnswer { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EnterNegativeMarks { get; set; }
        public int PerNoofQuestions { get; set; }
        public int NoofQuestionsforChoice { get; set; }
        public List<TestSeriesQuestionDifficulty>? TestSeriesQuestionDifficultyLevel { get; set; }
    }
}
