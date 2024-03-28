namespace Schools_API.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? DisplayOrder { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? SubjectType { get; set; }
    }
}
