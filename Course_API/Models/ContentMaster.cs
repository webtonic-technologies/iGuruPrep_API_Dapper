namespace Course_API.Models
{
    public class ContentMaster
    {
        public int Content_Id { get; set; }
        public int SubjectIndexId { get; set; }
        public int Board_Id { get; set; }
        public int Class_Id { get; set; }
        public int Course_Id { get; set; }
        public int Subject_Id { get; set; }
        public string NameOfFile { get; set; } = string.Empty;
        public string PathUrl { get; set; } = string.Empty;
    }
}
