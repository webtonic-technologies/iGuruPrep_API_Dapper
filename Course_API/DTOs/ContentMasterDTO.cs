namespace Course_API.DTOs
{
    public class ContentMasterDTO
    {

        public int Content_Id { get; set; }
        public int SubjectIndexId { get; set; }
        public int Board_Id { get; set; }
        public int Class_Id { get; set; }
        public int Course_Id { get; set; }
        public int Subject_Id { get; set; }
        public IFormFile? NameOfFile { get; set; }
        public IFormFile? PathUrl { get; set; }
    }
    public class ContentMasterFileDTO
    {
        public int Content_Id { get; set; }
        public IFormFile? NameOfFile { get; set; }
    }
}
