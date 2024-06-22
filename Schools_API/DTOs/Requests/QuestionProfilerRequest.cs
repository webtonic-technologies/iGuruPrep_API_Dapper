namespace Schools_API.DTOs.Requests
{
    public class QuestionProfilerRequest
    {
        public int QPID { get; set; }
        public int Questionid { get; set; }
        public int EmpId { get; set; }
        public bool? ApprovedStatus { get; set; }
        public bool? Status {  get; set; }
    }
}
