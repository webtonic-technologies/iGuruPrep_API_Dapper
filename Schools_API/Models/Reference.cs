namespace Schools_API.Models
{
    public class Reference
    {
        public int ReferenceId { get; set; }
        public int? SubjectIndexId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string ReferenceNotes { get; set; } = string.Empty;
        public string ReferenceURL { get; set; } = string.Empty;
        public int? QuestionId { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
