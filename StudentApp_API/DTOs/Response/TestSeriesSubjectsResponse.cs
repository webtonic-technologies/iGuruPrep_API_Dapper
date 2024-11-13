namespace StudentApp_API.DTOs.Response
{
    public class TestSeriesSubjectsResponse
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public class TestSeriesResponse
    {
        public int TestSeriesId { get; set; }
        public string TestPatternName { get; set; } = string.Empty;
        public string Duration { get; set; } // Duration in minutes
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime ResultDate { get; set; }
        public string ResultTime { get; set; }
        public int TotalNoOfQuestions { get; set; }
    }
    public class TestSeriesQuestionResponse
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public string QuestionFormula { get; set; } = string.Empty;
        public string QuestionImage { get; set; } = string.Empty;
        public int DifficultyLevelId { get; set; }
        public int QuestionTypeId { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public List<AnswerResponses> Answers { get; set; } = new List<AnswerResponses>();
    }

    public class AnswerResponses
    {
        public int AnswerId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

}
