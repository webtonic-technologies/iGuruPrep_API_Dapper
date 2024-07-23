namespace Schools_API.DTOs.Response
{
    public class QuestionComparisonDTO
    {
        public int QuestionID { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public double Similarity { get; set; }
    }
}
