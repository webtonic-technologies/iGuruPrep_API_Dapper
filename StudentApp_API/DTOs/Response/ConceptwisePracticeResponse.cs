namespace StudentApp_API.DTOs.Response
{
    public class ConceptwisePracticeResponse
    {
        public List<ConceptwisePracticeSubjectsResposne>?  conceptwisePracticeSubjectsResposnes { get; set; }
        public decimal Percentage { get; set; }
    }
    public class ConceptwisePracticeSubjectsResposne
    {
        public int SyllabusId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int? RegistrationId { get; set; }
        public decimal Percentage { get; set; }
    }
    public class ConceptwisePracticeContentResponse
    {
        public int SubjectId { get; set; }
        public int SyllabusId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentId { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public decimal Percentage { get; set; }
    }
    public class ConceptwiseAnswerResponse
    {
        public int QuestionID { get; set; }
        public string AnswerID { get; set; }
        public bool IsAnswerCorrect { get; set; }
    }
    public class QuestionAttemptStatsResponse
    {
        public int TotalAttempts { get; set; }  // Total students who attempted the question
        public int CorrectAnswers { get; set; } // Total students who answered correctly
    }
    public class PracticeStatsDto
    {
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int UnattemptedQuestions { get; set; }
        public int PartiallyCorrect { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAnswered { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal IncorrectPercentage { get; set; }
        public decimal UnattemptedPercentage { get; set; }
        public decimal PartiallyCorrectPercentage { get; set; }
    }
    public class AccuracyRateDto
    {
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public int TotalCorrectAttempts { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal OthersAccuracyRate { get; set; }
        public int HigherAccuracyCount { get; set; }
        public int AttemptedStudentsCount {  get; set; }
    }
    public class TimeSpentDto
    {
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public decimal TotalTimeSpent { get; set; } // In seconds
        public decimal AverageTimePerQuestion { get; set; } // In seconds
        public decimal ClassmatesTotalTimeSpent { get; set; } // Avg total time spent by classmates
        public decimal ClassmatesAverageTimePerQuestion { get; set; } // Avg time per question for classmates

    }
    public class AnswerTimeStatsDto
    {
        public decimal TotalTimeCorrect { get; set; }
        public decimal AverageTimeCorrect { get; set; }
        public decimal TotalTimeIncorrect { get; set; }
        public decimal AverageTimeIncorrect { get; set; }
        public decimal TotalTimePartiallyCorrect { get; set; }
        public decimal AverageTimePartiallyCorrect { get; set; }
        public int TotalUnattempted { get; set; }
    }
    public class AccuracyRateDtoComparison
    {
        public decimal StudentAccuracyRate { get; set; }
        public decimal TopperAccuracyRate { get; set; }
        public decimal AverageAccuracyOfOthers { get; set; }
    }
    public class StudentAccuracyDto
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal AccuracyRate { get; set; }
    }
}