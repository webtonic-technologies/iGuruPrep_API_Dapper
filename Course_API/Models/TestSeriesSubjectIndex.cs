using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class TestSeriesContentIndex
    {
        public int TestSeriesSubjectIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int TestSeriesID { get; set; }
        public int SubjectId {  get; set; }
    }
    public class TestSeriesAddTime
    {
        public int TestSeriesID { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime? StartDate { get; set; }
        public string? StartTime { get; set; } = string.Empty;
        public DateTime? ResultDate { get; set; }
        public string? ResultTime { get; set; } = string.Empty;
    }
}
