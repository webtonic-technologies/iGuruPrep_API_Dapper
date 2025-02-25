namespace StudentApp_API.DTOs.Requests
{
    //public class UpdateQuestionNavigationRequest
    //{
    //    public int ScholarshipID { get; set; }
    //    public int RegistrationId { get; set; }
    //    public int QuestionID { get; set; }
    //    public string StartTime { get; set; }
    //    public string EndTime { get; set; }
    //}
    public class UpdateQuestionNavigationRequest
    {
        public int StudentID { get; set; }
        public int ScholarshipID { get; set; }
        public int TotalTime { get; set; }
        public List<SubjectRequest> Subjects { get; set; }
    }
    public class GetScholarshipQuestionRequest
    {
        public int scholarshipTestId { get; set; }
        public int studentId { get; set; }
        public List<int>? QuestionTypeId { get; set; }
        public List<int>? QuestionStatus {  get; set; }
    }
    public class SubjectRequest
    {
        public int SubjectId { get; set; }
        public List<QuestionRequest> Questions { get; set; }
    }

    public class QuestionRequest
    {
        public int QuestionID { get; set; }
        public int QuestionTypeID { get; set; }
        public List<int> MultiOrSingleAnswerId { get; set; }
        public List<TimeLog> TimeLogs { get; set; }
    }

    public class TimeLog
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

}
