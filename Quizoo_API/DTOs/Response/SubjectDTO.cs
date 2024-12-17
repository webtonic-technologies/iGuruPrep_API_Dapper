namespace Quizoo_API.DTOs.Response
{

    public class SubjectDTO
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
    }

    public class ChapterDTO
    {
        public int ChapterId { get; set; }
        public string ChapterName { get; set; }
        public string ChapterCode { get; set; }
        public int DisplayOrder { get; set; }
    }

}
