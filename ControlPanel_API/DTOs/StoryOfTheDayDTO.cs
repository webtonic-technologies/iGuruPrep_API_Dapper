namespace ControlPanel_API.DTOs
{
    public class StoryOfTheDayDTO
    {
        public int? QuestionId { get; set; }
        public string Question { get; set; }
        public string BoardName { get; set; }
        public string ClassName { get; set; }
        public DateTime? PostTime { get; set; }
        public DateTime? DateAndTime { get; set; }
        public IFormFile? UploadImage { get; set; }
        public string Answer { get; set; }
        public TimeSpan? AnswerRevealTime { get; set; }
        public int Status { get; set; }
    }

    public class UpdateStoryOfTheDayDTO
    {
        public int? QuestionId { get; set; }
        public string Question { get; set; }
        public string BoardName { get; set; }
        public string ClassName { get; set; }
        public DateTime? PostTime { get; set; }
        public DateTime? DateAndTime { get; set; }
        public string Answer { get; set; }
        public TimeSpan? AnswerRevealTime { get; set; }
        public int Status { get; set; }
    }

    public class StoryOfTheDayIdAndFileDTO
    {
        public int? QuestionId { get; set; }
        public IFormFile? UploadImage { get; set; }
    }
}
