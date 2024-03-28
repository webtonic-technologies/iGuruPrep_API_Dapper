namespace ControlPanel_API.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public string FeedbackDesc { get; set; } = string.Empty;
        public Decimal? Rating { get; set; }
        public int? FeedbackTypeID { get; set; }
        public int? UserID { get; set; }
        public int? SyllabusID { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ParentfeedbackID { get; set; }
    }
}
