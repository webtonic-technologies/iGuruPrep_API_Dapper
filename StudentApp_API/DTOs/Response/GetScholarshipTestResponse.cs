using System.Collections.Generic;

namespace StudentApp_API.DTOs.Responses
{
    public class GetScholarshipTestResponseWrapper
    {
        public List<GetScholarshipTestResponse> ScholarshipDetails { get; set; }
    }

    public class GetScholarshipTestResponse
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public List<QuestionDetail> Questions { get; set; }
    }

    public class QuestionDetail
    {
        public int QuestionID { get; set; }
        public string Question { get; set; }
        public List<AnswerDetail> Answers { get; set; }
    }

    public class AnswerDetail
    {
        public int? Answermultiplechoicecategoryid {  get; set; }
        public int? Answersingleanswercategoryid {  get; set; }
        public int AnswerID { get; set; }
        public string Answer { get; set; }
    }
}
