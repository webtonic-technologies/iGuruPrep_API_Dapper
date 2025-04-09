using System.Globalization;

namespace StudentApp_API.DTOs.Response
{
    public class QuestionsSetResponse
    {
        public int? NumberOfSet { get; set; }
        public List<QuestionResponseDTO>? QuestionResponseDTOs { get; set; }
    }
    public class QuestionResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? subjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int? IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int? ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public bool? IsRejected { get; set; }
        public bool? IsApproved { get; set; }
        public string QuestionTypeName { get; set; } = string.Empty;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string userRole { get; set; } = string.Empty;
        public string? Paragraph { get; set; } = string.Empty;
        public int? ParentQId { get; set; }
        public string? ParentQCode { get; set; } = string.Empty;
        public int LevelId {  get; set; }
        public string LevelName {  get; set; } = string.Empty;
        public List<MatchPair>? MatchPairs { get; set; }
        public List<MatchThePairAnswer>? MatchThePairType2Answers { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public List<ParagraphQuestions>? ComprehensiveChildQuestions { get; set; }
        public int DurationperQuestion {  get; set; }
        public int QuestionStatusId {  get; set; }
    }
    public class ParagraphQuestions
    {
        public int? QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int? ParentQId { get; set; }
        public string ParentQCode { get; set; }
        public int? QuestionTypeId { get; set; }
        public bool? Status { get; set; }
        public int? CategoryId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? subjectID { get; set; }
        public int? EmployeeId { get; set; }
        public int? ModifierId { get; set; }
        public int? IndexTypeId { get; set; }
        public int? ContentIndexId { get; set; }
        public bool? IsRejected { get; set; } = false;
        public bool? IsApproved { get; set; } = false;
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsConfigure { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }
    public class Answersingleanswercategory
    {
        public int Answersingleanswercategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
    public class AnswerMultipleChoiceCategory
    {
        public int Answermultiplechoicecategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
        public bool? Iscorrect { get; set; }
        public int? Matchid { get; set; }
    }
    public class MatchThePairAnswer
    {
        public int MatchThePair2Id { get; set; }
        public int PairColumn { get; set; }
        public int PairRow { get; set; }
    }
    public class MatchPair
    {
        public int MatchThePairId { get; set; }
        public int PairColumn { get; set; }
        public int PairRow { get; set; }
        public string PairValue { get; set; }
    }

    public class AnswerPercentageResponse
    {
        public int QuizID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public int AnswerCount { get; set; }
        public double AnswerPercentage { get; set; }
        public string AnswerText { get; set; }  // Text of the answer
        public bool IsCorrect { get; set; }     // Correctness flag
    }

    public class SyllabusSubjectMapping
    {
        public int SyllabusID { get; set; }  // The ID of the syllabus
        public int SubjectID { get; set; }  // The ID of the subject mapped to the syllabus
    }

    // DTO class for the result
    public class QuestionWithCorrectAnswerDTO
    {
        public int QuizooID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionCode { get; set; }
        public string QuestionDescription { get; set; }
        public int MultiAnswerid { get; set; }
        public string Answer { get; set; }
    }

    // DTO class for the result
    public class StudentRankDTO
    {
        public int StudentID { get; set; }
        public int CorrectAnswers { get; set; }
        public string Country {  get; set; }
        public int Rank { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class CYOTQuestionWithAnswersDTO
    {
        public int CYOTID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionCode { get; set; }
        public int QuestionStatusId {  get; set; }
        public int QuestionTypeId {  get; set; }
        public string QuestionType {  get; set; }
        public string QuestionDescription { get; set; }
        public string Explanation {  get; set; }
        public string ExtraInformation {  get; set; }
        public string SubjectName {  get; set; }
        public List<AnswerOptionDTO> Answers { get; set; } // List of answers for the question
    }
    public class AnswerOptionDTO
    {
        public int AnswerMultipleChoiceCategoryID { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsStudentAnswer { get; set; }  // New field to indicate if this answer was chosen by the student
        public bool IsStudentAnswerCorrect { get; set; }  // New field to indicate whether the given answer was correct
    }
    public class AnswerMasters
    {
        public int Answerid { get; set; }
        public int Questionid { get; set; }
        public int QuestionTypeid { get; set; }
    }
    // DTOs for the queries
    public class DifficultyLevelDTO
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public int NoofQperLevel { get; set; }
    }
    public class CYOTQestionReportResponse
    {
        public int TotalQuestions { get; set; }
        public string TotalDuration { get; set; }
        public decimal TotalMarks { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public int UnansweredCount { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal IncorrectPercentage { get; set; }
        public decimal UnansweredPercentage { get; set; }
    }
    public class CYOTAnalyticsResponse
    {
        public decimal? AchievedMarks { get; set; }
        public decimal? NegativeMarks { get; set; }
        public decimal? FinalMarks { get; set; }
        public decimal? FinalPercentage { get; set; }
    }
    public class CYOTTimeAnalytics
    {
        public string? TotalTimeSpent { get; set; }            // Total time (all questions) in minutes
        public string? AvgTimePerQuestion { get; set; }          // Average time per question in minutes

        public string? TotalTimeSpentCorrect { get; set; }       // Total time for correct answers in minutes
        public string? AvgTimeSpentCorrect { get; set; }         // Average time for correct answers in minutes

        public string? TotalTimeSpentWrong { get; set; }         // Total time for wrong answers in minutes
        public string? AvgTimeSpentWrong { get; set; }           // Average time for wrong answers in minutes

        public string? TotalTimeSpentUnattempted { get; set; }   // Total time for unattempted questions in minutes
        public string? AvgTimeSpentUnattempted { get; set; }     // Average time for unattempted questions in minutes
    }
    public class CYOTPerformancePerSubjectDTO
    {
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public int UnansweredCount { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal IncorrectPercentage { get; set; }
        public decimal UnansweredPercentage { get; set; }
        public decimal AchievedMarks { get; set; }
        public decimal NegativeMarks { get; set; }
        public decimal FinalMarks { get; set; }
        public decimal FinalPercentage { get; set; }
    }
    public class CYOTTimeCategoryAnalytics
    {
        public decimal TotalTimeSpent { get; set; }
        public decimal AvgTimePerQuestion { get; set; }
    }

    public class CYOTSubjectTimeAnalytics
    {
        public decimal TotalTimeAll { get; set; }
        public decimal AvgTimeAll { get; set; }
        public CYOTTimeCategoryAnalytics Correct { get; set; }
        public CYOTTimeCategoryAnalytics Incorrect { get; set; }
        public CYOTTimeCategoryAnalytics Unattempted { get; set; }
    }
}
