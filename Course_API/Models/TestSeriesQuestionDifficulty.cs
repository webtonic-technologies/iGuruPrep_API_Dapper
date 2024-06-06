namespace Course_API.Models
{
    public class TestSeriesQuestionDifficulty
    {
        public int TestSeriesQuestionDifficultyId { get; set; }
        public int LevelID { get; set; }
        public int TestSeriesID { get; set; }
        public decimal PercentagePerDifficulty { get; set; }
    }
}
