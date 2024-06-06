namespace Course_API.Models
{
    public class TestSeriesQuestionSection
    {
        public int testseriesQuestionSectionid { get; set; }
        public int TestSeriesid { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool? Status { get; set; }
    }
}
