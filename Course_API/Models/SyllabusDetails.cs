namespace Course_API.Models
{
    public class SyllabusDetails
    {
        public int SyllabusDetailID { get; set; }
        public int SyllabusID { get; set; }
        public int SubjectIndexID { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DisplayOrder { get; set; }
        public int? IsVerson { get; set; }
    }
}
