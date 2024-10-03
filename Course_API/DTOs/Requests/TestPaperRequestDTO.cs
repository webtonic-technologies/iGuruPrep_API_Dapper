namespace Course_API.DTOs.Requests
{
    public class TestPaperRequestDTO
    {
        public int TestPaperId { get; set; }
        public int APID { get; set; }
        public int ExamTypeId { get; set; }
        public string PatternName { get; set; } = string.Empty;
        public int TotalNumberOfQuestions { get; set; }
        public string Duration { get; set; } = string.Empty;
        public bool Status { get; set; }
        public string NameOfExam { get; set; } = string.Empty;
        public DateTime? ConductedDate { get; set; }
        public string ConductedTime { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public List<TestPaperBoard>? TestPaperBoards { get; set; }
        public List<TestPaperClass>? TestPaperClasses { get; set; }
        public List<TestPaperCourse>? TestPaperCourses { get; set; }
        public List<TestPaperSubject>? TestPaperSubjects { get; set; }
    }
    public class TestPaperBoard
    {
        public int TestPaperBoardId { get; set; }
        public int TestPaperId { get; set; }
        public int BoardId { get; set; }
    }
    public class TestPaperClass
    {
        public int TestPaperClassId { get; set; }
        public int TestPaperId { get; set; }
        public int ClassId { get; set; }
    }
    public class TestPaperCourse
    {
        public int TestPaperCourseId { get; set; }
        public int TestPaperId { get; set; }
        public int CourseId { get; set; }
    }
    public class TestPaperSubject
    {
        public int TestPaperSubjectId { get; set; }
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
    }
    public class TestPaperContentIndex
    {
        public int TestPaperContIndId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
    }
    public class TestPaperQuestionSection
    {
        public int TestPaperSectionId { get; set; }
        public int TestPaperId { get; set; }
        public int DisplayOrder { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public int TotalNumberOfQuestions { get; set; }
        public decimal MarksPerQuestion { get; set; }
        public decimal NegativeMarks { get; set; }
        public int NoOfQuestionsPerChoice { get; set; }
        public int LevelId1 { get; set; }
        public int QuesPerDifficulty1 { get; set; }
        public int LevelId2 { get; set; }
        public int QuesPerDifficulty2 { get; set; }
        public int LevelId3 { get; set; }
        public int QuesPerDifficulty3 { get; set; }
        public int SubjectId { get; set; }
    }
    public class TestPaperInstructions
    {
        public int TestPaperInstructionsId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public int TestPaperId { get; set; }
    }
    public class TestPaperQuestions
    {
        public int TestPaperQuestionsId { get; set; }
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
        public int DisplayOrder { get; set; }
        public int TestPaperSectionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
    }
    public class TestPaperGetListRequest
    {
        public int APId { get; set; }
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int ExamTypeId { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
