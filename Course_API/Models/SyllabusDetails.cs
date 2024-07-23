namespace Course_API.Models
{
    public class SyllabusDetails
    {
        public int SyllabusDetailID { get; set; }
        public int SyllabusID { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int Status { get; set; }
        public int? IsVerson { get; set; }
        public string Synopsis { get; set; } = string.Empty;
    }
}
