namespace ControlPanel_API.DTOs
{
    public class StoryOfTheDayDTO
    {
        public int? StoryId { get; set; }
        public int Questionid { get; set; }
        public string QuestionName { get; set; } = string.Empty;
        public string BoardID { get; set; } = string.Empty;
        public string ClassID { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime? PostTime { get; set; }
        public DateTime? DateAndTime { get; set; }
        public IFormFile? UploadImage { get; set; }
        public string Answer { get; set; } = string.Empty;
        public TimeSpan? AnswerRevealTime { get; set; }
        public int Status { get; set; }
    }

    public class UpdateStoryOfTheDayDTO
    {
        public int? StoryId { get; set; }
        public int Questionid { get; set; }
        public string QuestionName { get; set; } = string.Empty;
        public string BoardID { get; set; } = string.Empty;
        public string ClassID { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime? PostTime { get; set; }
        public DateTime? DateAndTime { get; set; }
        public string Answer { get; set; } = string.Empty;
        public TimeSpan? AnswerRevealTime { get; set; }
        public int Status { get; set; }
    }

    public class StoryOfTheDayIdAndFileDTO
    {
        public int? StoryId { get; set; }
        public IFormFile? UploadImage { get; set; }
    }
}
