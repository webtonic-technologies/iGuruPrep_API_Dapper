namespace Schools_API.DTOs.Requests
{
    public class ReportedQuestionQueryRequest
    {
        public int QueryCode { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string Reply { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string ImageOrPDF { get; set; } = string.Empty;
        public int EmployeeId {  get; set; }
    }
    public class ReportedQuestionRequest
    {
        public int? SubjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
        public int EmployeeId { get; set; }
    }
    public class ReportedQuestionRequestDTO
    {
        public int QueryCode { get; set; }
        public string Querydescription { get; set; } = string.Empty;
        public string QuestionCode { get; set; } = string.Empty;
        public DateTime? DateandTime { get; set; }
        public int RQSID { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string ImageOrPDF { get; set; } = string.Empty;
        public int subjectID { get; set; }
        public int StudentId { get; set; }
        public int EmployeeId { get; set; }
        public int CategoryId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int BoardId { get; set; }
        public int ExamTypeId { get; set; }
    }
    public class RQStatusRequest
    {
        public int QueryCode { get; set; }
        public int EmployeeId { get; set; }
    }
}