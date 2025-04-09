using System.Diagnostics.CodeAnalysis;

namespace StudentApp_API.DTOs.Requests
{
    public class CYOTDTO
    {
        public int CYOTID { get; set; }
        public string ChallengeName { get; set; }
        public DateTime ChallengeDate { get; set; }
        public DateTime ChallengeStartTime { get; set; }
        public string Duration { get; set; }
        public int NoOfQuestions { get; set; }
        public int MarksPerCorrectAnswer { get; set; }
        public int MarksPerIncorrectAnswer { get; set; }
        public int CreatedBy { get; set; }
        public List<CYOTSyllabusDTO> CYOTSyllabus { get; set; }
    }
    public class CYOTListRequest
    {
        public int RegistrationId { get; set; }
        public int StatusId { get; set; }
    }
    public class GetCYOTQuestionsRequest
    {
        public int cyotId { get; set; }
        public int registrationId { get; set; }
        public List<int>? QuestionTypeId { get; set; }
        public List<int>? QuestionStatusId {  get; set; }
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
    public class CYOTParticipantRequest
    {
        public int StudentID { get; set; }
        public int CYOTID { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsStarted { get; set; }
        public int CYOTStatusID { get; set; }
    }
    public class SaveQuestionCYOTRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
        public int CYOTId { get; set; }
    }
    public class CYOTQuestionNavigationRequest
    {
        public int StudentID { get; set; }
        public int CYOTId { get; set; }
        public int TotalTime { get; set; }
        public List<CYOTSubjectRequest> Subjects { get; set; }
    }
    public class CYOTSubjectRequest
    {
        public int SubjectId { get; set; }
        public List<CYOTQuestionRequest> Questions { get; set; }
    }
    public class CYOTQuestionRequest
    {
        public int QuestionID { get; set; }
        public int QuestionTypeID { get; set; }
        public int QuestionstatusId { get; set; }
        public List<int>? MultiOrSingleAnswerId { get; set; }
        public string? SubjectiveAnswers { get; set; } = string.Empty;
        public List<MatchThePairAnswer>? MatchThePairAnswers { get; set; }
        public List<TimeLog> TimeLogs { get; set; }
    }
    public class MatchThePairAnswer
    {
       // public int MatchThePair2Id { get; set; }
        public int PairColumn { get; set; }
        public int PairRow { get; set; }
    }
    public class CYOTAnswerSubmissionRequest
    {
        public int CYOTId { get; set; }
        public int RegistrationId { get; set; }
        public int QuestionID { get; set; }
        public List<int>? MultiOrSingleAnswerId { get; set; }
        public string? SubjectiveAnswers { get; set; } = string.Empty;
        public List<MatchThePairAnswer>? MatchThePairAnswers {  get; set; }
        public int SubjectID { get; set; }
        public int QuestionTypeID { get; set; }
    }
    public class StudentData
    {
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
    }
    public class MarksResult
    {
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int AttemptedQuestions { get; set; }
        public int TotalQuestions { get; set; }
    }
    public class GetChaptersRequestCYOT
    {
        public int registrationId {  get; set; }
        public List<int> SubjectIds { get; set; }
    }
}
