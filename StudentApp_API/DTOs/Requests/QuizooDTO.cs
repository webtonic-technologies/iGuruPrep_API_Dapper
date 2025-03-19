namespace StudentApp_API.DTOs.Requests
{
    public class QuizooDTO
    {
        public int QuizooID { get; set; }
        public string QuizooName { get; set; }
        public DateTime QuizooDate { get; set; }
        public DateTime QuizooStartTime { get; set; }
        public string Duration { get; set; }
        public int NoOfQuestions { get; set; }
        public int NoOfPlayers { get; set; }
        public string QuizooLink { get; set; }
        public int CreatedBy { get; set; }
        public bool IsSystemGenerated { get; set; }
        public List<QuizooSyllabusDTO> QuizooSyllabus { get; set; }
    }
    public class OnlineQuizooDTO
    {
        public int CreatedBy { get; set; }
    }
    public class QuizooSyllabusDTO
    {
        //  public int QSID { get; set; }
        public int QuizooID { get; set; }
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
    }
    public class SubmitAnswerRequest
    {
        public int QuizID { get; set; }
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //      public bool IsCorrect { get; set; }
    }
    public class QuizooDTOResponse
    {
        public int QuizooID { get; set; }
        public string QuizooName { get; set; }
        public DateTime QuizooDate { get; set; }
        public DateTime QuizooStartTime { get; set; }
        public string Duration { get; set; }
        public int NoOfQuestions { get; set; }
        public int NoOfPlayers { get; set; }
        public string QuizooLink { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        //public string QuizooDuration { get; set; }
        public bool IsSystemGenerated { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int BoardID { get; set; }
        public string QuizooStatus { get; set; }
        public int Players { get; set; }
        public bool ShowLeaderBoard { get; set; } = false;
        public bool ShowCorrectAnswers { get; set; } = false;
        public List<QuizooSyllabusDTO> QuizooSyllabus { get; set; }
    }
    public class ParticipantDto
    {
        public int StudentId { get; set; }
        public bool IsForceExit { get; set; }
        public string FullName { get; set; }
        public string Photo { get; set; } // Assuming Photo is stored as a Base64 string or URL
    }
    public class QuizooListFilters
    {
        public int RegistrationId { get; set; }
        public List<int> Filters { get; set; }
    }

    public enum QuizooFilterType
    {
        Upcoming = 1,
        NotJoined = 2,
        Ongoing = 3,
        Completed = 4,
        Missed = 5
    }
}
