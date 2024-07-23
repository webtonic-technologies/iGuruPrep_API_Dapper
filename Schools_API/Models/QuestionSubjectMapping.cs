namespace Schools_API.Models
{
    public class QuestionSubjectMapping
    {
        public int QuestionSubjectid { get; set; }
        public int ContentIndexId { get; set; }
        public int Indexid { get; set; }
        public int questionid { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        //public int Levelid { get; set; }
        // public int SubjectIndexId { get; set; }
    }
}
