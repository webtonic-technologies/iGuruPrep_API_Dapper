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
}
