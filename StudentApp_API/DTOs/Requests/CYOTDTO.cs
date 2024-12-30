namespace StudentApp_API.DTOs.Requests
{
    public class CYOTDTO
    {
        public int CYOTID { get; set; }
        public string ChallengeName { get; set; }
        public DateTime ChallengeDate { get; set; }
        public DateTime ChallengeStartTime { get; set; }
        public int? Duration { get; set; }
        public int? NoOfQuestions { get; set; }
        public int? MarksPerCorrectAnswer { get; set; }
        public decimal? MarksPerIncorrectAnswer { get; set; }
        public int? CreatedBy { get; set; }
        public int? ClassID { get; set; }
        public int? CourseID { get; set; }
        public int? BoardID { get; set; }
        public List<CYOTSyllabusDTO> CYOTSyllabus { get; set; }
    }
    public class GetCYOTQuestionsRequest
    {
        public int cyotId { get; set; }
        public int registrationId { get; set; }
        public List<int>? QuestionTypeId { get; set; }
    }
    public class CYOTSyllabusDTO
    {
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
    }

    public class CYOTQuestionsDTO
    {
        public int QuestionID { get; set; }
        public int DisplayOrder { get; set; }
    }

}
