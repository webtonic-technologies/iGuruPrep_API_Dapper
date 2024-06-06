namespace Course_API.Models
{
    public class TestSeries
    {
        public int TestSeriesId { get; set; }
        public int boardid { get; set; }
        public int classId { get; set; }
        public int CourseId { get; set; }
        public int ExamTypeID { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public string TestPatternName { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ExamTypeName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
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
    }
}
