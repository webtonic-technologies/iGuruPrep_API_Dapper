namespace Course_API.Models
{
    public class TestSeriesBoards
    {

        public int TestSeriesBoardsId { get; set; }
        public int TestSeriesId { get; set; }
        public int BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
    }
}
