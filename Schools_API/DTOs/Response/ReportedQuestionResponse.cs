namespace Schools_API.DTOs.Response
{
    public class ReportedQuestionResponse
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
        public string subjectname { get; set; } = string.Empty;
        public string RQSName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; private set; } = string.Empty;
        public int BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
    }
}
