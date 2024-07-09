namespace ControlPanel_API.DTOs.Requests
{
    public class GeAllContactUsRequest
    {
        public int? BoardID { get; set; }
        public int? CourseId { get; set; }
        public int? ClassId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
        public int APID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchText { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
    }
}
