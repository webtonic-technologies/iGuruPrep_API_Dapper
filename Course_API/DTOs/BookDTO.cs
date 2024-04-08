namespace Course_API.DTOs
{
    public class BookDTO
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorDetails { get; set; } = string.Empty;
        public string AuthorAffliation { get; set; } = string.Empty;
        public string Boardname { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
