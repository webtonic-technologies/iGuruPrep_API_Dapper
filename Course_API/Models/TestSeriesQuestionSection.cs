using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class TestSeriesQuestionSection
    {
        public int testseriesQuestionSectionid { get; set; }
        public int TestSeriesid { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        [Required(ErrorMessage = "Difficulty level cannot be empty")]
        public int LevelID1 { get; set; }
        public int QuesPerDifficulty1 { get; set; }
        [Required(ErrorMessage = "Difficulty level cannot be empty")]
        public int LevelID2 { get; set; }
        public int QuesPerDifficulty2 { get; set; }
        [Required(ErrorMessage = "Difficulty level cannot be empty")]
        public int LevelID3 { get; set; }
        public int QuesPerDifficulty3 { get; set; }
        public int QuestionTypeID { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EntermarksperCorrectAnswer { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EnterNegativeMarks { get; set; }
        public int TotalNoofQuestions {  get; set; }
        public int NoofQuestionsforChoice {  get; set; }
    }
}