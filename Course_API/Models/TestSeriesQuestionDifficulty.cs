using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class TestSeriesQuestionDifficulty
    {
        public int TestSeriesQuestionDifficultyId { get; set; }
        [Required(ErrorMessage = "Difficulty level cannot be empty")]
        public int LevelID { get; set; }
        public int TestSeriesID { get; set; }
        public decimal PercentagePerDifficulty { get; set; }
    }
}
