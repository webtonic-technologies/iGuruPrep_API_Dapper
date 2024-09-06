namespace Schools_API.DTOs.Requests
{
    public class QuestionProfilerRequest
    {
        public int QPID { get; set; }
        public int Questionid { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int EmpId { get; set; }
        public bool? ApprovedStatus { get; set; }
        public bool? Status {  get; set; }
        public DateTime AssignedDate {  get; set; }
    }
    public class SyllabusDetailsRequest
    {
        public int APId { get; set; } //if APId is 1 then board, class, course will have data and exam type will be 0 , if APId is 2 then board,class,course will be 0 and exam type will have data
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int ExamTypeId { get; set; }
        public int SubjectId { get; set; }
    }
    public class DownExcelRequest
    {
        public int subjectId { get; set; }
        public int indexTypeId { get; set; } //1 means chapter, 2 means topic and 3 means sub topic
        public int contentId { get; set; }
    }
}
