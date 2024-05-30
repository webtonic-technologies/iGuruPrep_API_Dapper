namespace Course_API.Models
{
    public class SyllabusDetails
    {
        public int SyllabusDetailID { get; set; }
        public int SyllabusID { get; set; }
        public int SubjectIndexID { get; set; }
        public int Status { get; set; }
        public int? IsVerson { get; set; }
    }
}
