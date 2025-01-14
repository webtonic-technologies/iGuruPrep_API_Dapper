namespace Schools_API.DTOs.Response
{
    public class QuestionProfilerResponse
    {
        public int QPID { get; set; }
        public int SubjectId {  get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public string QuestionTypeName { get; set; } = string.Empty;
        public List<QuestionContentDetails>? QuestionContentDetails { get; set; }
        public int Questionid { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public bool? ApprovedStatus { get; set; }
        public List<ProoferList>? Proofers { get; set; }
        public List<QIDCourseResponse>? QIDCourses { get; set; }
        public List<QuestionRejectionResponseDTO>? QuestionRejectionResponseDTOs { get; set; }
    }
    public class QuestionContentDetails
    {
        public int IndexTypeId { get; set; }
        public string IndexTypeName {  get; set; } = string.Empty;
        public int ContentIndexId {  get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
    }
    public class QuestionRejectionResponseDTO
    {
        public int RejectionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int EmpId { get; set; }
        public string EmpName { get; set; } = string.Empty;
        public DateTime? RejectedDate { get; set; }
        public string RejectedReason { get; set; } = string.Empty;
    }
    public class ProoferList
    {
        public int? QPId { get; set; }
        public string QuestionCode { get; set; }=string.Empty;
        public int? AssignedToEmpId { get; set; }
        public string AssignedToEmpName { get; set; } = string.Empty;
        public string AssignedToRole { get; set; } = string.Empty;
        public int? AssignedToRoleId { get; set; }
        public int? AssignedByEmpId { get; set; }
        public string AssignedByEmpName { get; set; } = string.Empty;
        public DateTime? AssignedDate {  get; set; }
    }
    public class QuestionComparisonResponse
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; }
        public string QuestionDescription { get; set; }
        public string Explanation { get; set; }
        public string ExtraInformation { get; set; }
        public string ActualOption { get; set; }
        public bool ApprovedStatus { get; set; }
        public int DifficultyLevelId { get; set; }
        public int CourseId { get; set; }
        public int BoardId { get; set; }
        public int SubjectId { get; set; }
        public bool IsActive { get; set; }
    }
    public class EmployeeListAssignedQuestionCount
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Count {  get; set; }
    }
}
