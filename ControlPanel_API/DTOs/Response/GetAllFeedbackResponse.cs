namespace ControlPanel_API.DTOs
{
    public class GetAllFeedbackResponse
    {
        public int FeedBackId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FeedBackDesc { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public DateTime? Date { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BoardId { get; set; }
        public string Board { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string Class { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string Course { get; set; } = string.Empty;
        public int APID { get; set; }
        public string APName { get; set; } = string.Empty;
        public int ExamTypeId {  get; set; }
        public string ExamTypeName {  get; set; } = string.Empty;
    }
}   