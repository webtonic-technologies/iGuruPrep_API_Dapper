namespace ControlPanel_API.DTOs
{
    public class GetAllFeedbackRequest
    {
        public int? BoardID { get; set; }
        public int? CourseId { get; set; }
        public int? ClassId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
    }
}
