namespace Course_API.Models
{
    public class TestSeriesCourse
    {
        public int TestSeriesCourseId { get; set; }
        public int TestSeriesId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }
}
