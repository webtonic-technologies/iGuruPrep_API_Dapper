namespace Course_API.DTOs
{
    public class BookDTO
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string AuthorName { get; set; }
        public string AuthorDetails { get; set; }
        public string AuthorAffliation { get; set; }
        public string Boardname { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public string SubjectName { get; set; }
        public int Status { get; set; }
    }
}
