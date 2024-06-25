using Course_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Course_API.DTOs.Response
{
    public class TestSeriesResponseDTO
    {
        public int TestSeriesId { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public string TestPatternName { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int APID { get; set; }
        public string APName { get; set; } = string.Empty;
        public int TotalNoOfQuestions { get; set; }
        public bool MethodofAddingType { get; set; }
        public DateTime? StartDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public DateTime? ResultDate { get; set; }
        public string ResultTime { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string NameOfExam { get; set; } = string.Empty;
        public bool RepeatedExams { get; set; }
        public int TypeOfTestSeries {  get; set; }
        public string TypeOfTestSeriesName {  get; set; } = string.Empty;
        public List<TestSeriesBoardsResponse>? TestSeriesBoard { get; set; }
        public List<TestSeriesClassResponse>? TestSeriesClasses { get; set; }
        public List<TestSeriesCourseResponse>? TestSeriesCourses { get; set; }
        public List<TestSeriesSubjectsResponse>? TestSeriesSubject { get; set; }
        public List<TestSeriesContentIndexResponse>? TestSeriesContentIndexes { get; set; }
        public TestSeriesQuestionSection? TestSeriesQuestionsSection { get; set; }
        public List<TestSeriesQuestionType>? TestSeriesQuestionTypes { get; set; }
        public List<TestSeriesInstructions>? TestSeriesInstruction { get; set; }
        public List<TestSeriesQuestions>? TestSeriesQuestions { get; set; }
    }
    public class TestSeriesBoardsResponse
    {
        public int TestSeriesBoardsId { get; set; }
        public int TestSeriesId { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestSeriesClassResponse
    {
        public int TestSeriesClassesId { get; set; }
        public int TestSeriesId { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestSeriesCourseResponse
    {
        public int TestSeriesCourseId { get; set; }
        public int TestSeriesId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TestSeriesSubjectsResponse
    {
        public int TestSeriesSubjectId { get; set; }
        public int SubjectID { get; set; }
        public int TestSeriesID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int NoOfQuestions { get; set; }
    }
    public class TestSeriesContentIndexResponse
    {
        public int TestSeriesSubjectIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public string IndexTypeName {  get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public int TestSeriesID { get; set; }
    }
}
