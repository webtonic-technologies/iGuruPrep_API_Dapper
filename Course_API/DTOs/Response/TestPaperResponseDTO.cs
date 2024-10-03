using Course_API.DTOs.Requests;

namespace Course_API.DTOs.Response
{
    public class TestPaperResponseDTO
    {
        public int TestPaperId { get; set; }
        public int APID { get; set; }
        public string APName { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
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
        public string EmployeeName { get; set; } = string.Empty;
        public List<TestPaperBoardResponse>? TestPaperBoards { get; set; }
        public List<TestPaperClassResponse>? TestPaperClasses { get; set; }
        public List<TestPaperCourseResponse>? TestPaperCourses { get; set; }
        public List<TestPaperSubjectResponse>? TestPaperSubjects { get; set; }
        public List<TestPaperSubjectDetails>? TestPaperSubjectDetails { get; set; }
        public List<TestPaperInstructions>? TestPaperInstruction { get; set; }
        public List<TestPaperQuestions>? TestPaperQuestions { get; set; }
    }
    public class TestPaperBoardResponse
    {
        public int TestPaperBoardId { get; set; }
        public int TestPaperId { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestPaperClassResponse
    {
        public int TestPaperClassId { get; set; }
        public int TestPaperId { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestPaperCourseResponse
    {
        public int TestPaperCourseId { get; set; }
        public int TestPaperId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestPaperSubjectResponse
    {
        public int TestPaperSubjectId { get; set; }
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestPaperContentIndexResponse
    {
        public int TestPaperContIndId { get; set; }
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public int TestPaperId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
    public class TestPaperSubjectDetails
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<TestPaperContentIndexResponse>? TestSeriesContentIndexes { get; set; }
        public List<TestPaperQuestionSection>? TestSeriesQuestionsSection { get; set; }
    }
}