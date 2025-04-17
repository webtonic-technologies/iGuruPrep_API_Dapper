namespace StudentApp_API.DTOs.Response
{
    public class PreviousPapersResponse
    {
    }
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
    }
    public class TestSeriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
    }
    public class TestSeriesSubjects
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<ContentIndexResponse> TestSeriesContentIndexes { get; set; } = new();
    }

    public class ContentIndexResponse
    {
        public int ContentIndexId { get; set; }
        public string ContentName_Chapter { get; set; } = string.Empty;
        public List<ContentIndexTopics> ContentIndexTopics { get; set; } = new();
    }

    public class ContentIndexTopics
    {
        public int ContInIdTopic { get; set; }
        public string ContentName_Topic { get; set; } = string.Empty;
        public List<ContentIndexSubTopic> ContentIndexSubTopics { get; set; } = new();
    }

    public class ContentIndexSubTopic
    {
        public int ContInIdSubTopic { get; set; }
        public string ContentName_SubTopic { get; set; } = string.Empty;
    }
    public class TestSeriesInstructions
    {
        public int TestInstructionsId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public int TestSeriesID { get; set; }
        public string InstructionName { get; set; } = string.Empty;
        public int InstructionId { get; set; }
    }

    public class TestSeriesResponseDTO
    {
        public int TestSeriesId { get; set; }
        public string TestPatternName { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int TotalNoOfQuestions { get; set; }
        public bool ManualQuestionSelect { get; set; }
        public DateTime? StartDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public DateTime? ResultDate { get; set; }
        public TestSeriesInstructions? TestSeriesInstruction { get; set; }
    }



    public class PreviousPaperTestSeriesSubjectDetails
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<QuestionSection>? TestSeriesQuestionsSection { get; set; }
    }
    public class QuestionSection
    {
        public int SectionId { get; set; }
        public string SectionName {  get; set; } = string.Empty;
        public int SubjectId {  get; set; }
        public int QuestionTypeId {  get; set; }
        public List<TestSeriesQuestions> QuestionsList {  get; set; }
    }
    public class TestSeriesQuestions
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int? QuestionTypeId { get; set; }
        public int? subjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int? IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int? ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public string QuestionTypeName { get; set; } = string.Empty;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public string? Paragraph { get; set; } = string.Empty;
        public int? ParentQId { get; set; }
        public string? ParentQCode { get; set; } = string.Empty;
        public List<MatchPair>? MatchPairs { get; set; }
        public List<MatchThePairAnswer>? MatchThePairType2Answers { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
        public List<ParagraphQuestions>? ComprehensiveChildQuestions { get; set; }
    }
}