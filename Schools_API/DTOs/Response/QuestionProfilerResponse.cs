namespace Schools_API.DTOs.Response
{
    public class QuestionProfilerResponse
    {

        public int QPID { get; set; }
        public int Questionid { get; set; }
        public bool? ApprovedStatus { get; set; }
        public List<ProoferList>? Proofers { get; set; }
        public List<QIDCourseResponse>? QIDCourses { get; set; }
        public List<QuestionRejectionResponseDTO>? QuestionRejectionResponseDTOs { get; set; }
    }
    public class QuestionRejectionResponseDTO
    {
        public int RejectionId { get; set; }
        public int QuestionId { get; set; }
        public int EmpId { get; set; }
        public string EmpName { get; set; } = string.Empty;
        public DateTime? RejectedDate { get; set; }
        public string RejectedReason { get; set; } = string.Empty;
    }
    public class ProoferList
    {
        public int QPId {  get; set; }
        public int EmpId { get; set; }
        public string EmpName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}
