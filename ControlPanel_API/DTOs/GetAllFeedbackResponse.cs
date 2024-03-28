namespace ControlPanel_API.DTOs
{
    public class GetAllFeedbackResponse
    {
       public string Name { get; set; } = string.Empty;
        public string FeedBackDesc { get; set; } = string.Empty;
        public Decimal? Rating {  get; set; }
        public DateTime? Date { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;  
        public string Board { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
    }
}