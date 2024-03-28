namespace ControlPanel_API.DTOs
{
    public class GetAllFeedbackRequest
    {
        public string Boardname { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
    }
}
