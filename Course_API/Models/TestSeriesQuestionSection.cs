using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class QuestionSection
    {
        public int SubjectId { get; set; }
        public List<TestSeriesQuestionSectionRequest>? TestSeriesQuestionSections { get; set; }
    }
    public class TestSeriesQuestionSection
    {
        public int testseriesQuestionSectionid { get; set; }
        public int TestSeriesid { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool? Status { get; set; }
      
        public List<TestSeriesQuestionDifficulty>? TestSeriesQuestionDifficulties { get; set; }
        public int QuestionTypeID { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EntermarksperCorrectAnswer { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EnterNegativeMarks { get; set; }
        public int TotalNoofQuestions {  get; set; }
        public int NoofQuestionsforChoice {  get; set; }
        public int SubjectId { get; set; }
        public int PartialMarkRuleId {  get; set; }
    }
    public class TestSeriesQuestionSectionRequest
    {
        public int testseriesQuestionSectionid { get; set; }
        public int TestSeriesid { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int QuestionTypeID { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EntermarksperCorrectAnswer { get; set; }
        [Required(ErrorMessage = "Mark per question cannot be empty")]
        public decimal EnterNegativeMarks { get; set; }
        public int TotalNoofQuestions { get; set; }
        public int NoofQuestionsforChoice { get; set; }
        public List<TestSeriesQuestionDifficulty>? TestSeriesQuestionDifficulties { get; set; }
    }
    public class TestSeriesQuestionDifficulty
    {
        public int Id {  get; set; }
        public int QuestionSectionId { get; set; }
        public int DifficultyLevelId { get; set; }
        public int QuesPerDiffiLevel { get; set; }
    }
}