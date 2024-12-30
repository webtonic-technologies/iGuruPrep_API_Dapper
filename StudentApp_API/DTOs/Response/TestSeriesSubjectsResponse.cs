namespace StudentApp_API.DTOs.Response
{
    public class TestSeriesSubjectsResponse
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage {  get; set; }
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
        public decimal Percentage { get; set; }
    }
    public class TestSeriesQuestionsList
    {
        public int TestSeriesid {  get; set; }
        public int testseriesQuestionSectionid {  get; set; }
        public int SubjectId { get; set; }
        public List<TestSeriesQuestionResponse>? TestSeriesQuestionResponses { get; set; }
    }
    public class TestSeriesQuestionMapping
    {
        public int Questionid { get; set; }
        public int testseriesQuestionSectionid { get; set; }
    }

    public class TestSeriesQuestionResponse
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; }
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
    public class QuestionTypeResponse
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; }
    }
    public class TestSeriesSubjectDetails
    {
        public int TestSeriesId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }

    public class TestSeriesQuestionDetails
    {
        public int QuestionId { get; set; }
        public int TotalQuestions { get; set; }
        public bool Bookmarked { get; set; }
    }
    public class QuestionTypeDTO
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
    }

}
