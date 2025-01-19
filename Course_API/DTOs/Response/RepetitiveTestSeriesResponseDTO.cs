namespace Course_API.DTOs.Response
{
    public class RepetitiveTestSeriesResponseDTO
    {
        public DateTime RepetitionDate { get; set; }
        public List<TestSeriesResponseDTOs> Sections { get; set; }
    }
    public class TestSeriesResponseDTOs
    {
        public string SectionName { get; set; }
        public int SectionId { get; set; }
        public List<QuestionResponseDTO> Questions { get; set; }
    }
}
