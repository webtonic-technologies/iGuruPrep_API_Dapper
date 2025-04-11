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
        public int ChapterCount {  get; set; }
    }
    public class ConceptwisePracticeContentResponse
    {
        public int? SubjectId { get; set; }
        public int SyllabusId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentId { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public decimal Percentage { get; set; }
        public int ConceptOrSubConceptCount {  get; set; }
        public int AttemptCount {  get; set; }
        public bool IsSynopsis {  get; set; }
        public bool IsAnalytics {  get; set; }
        public bool IsQuestionAnalytics {  get; set; }
    }
    public class ConceptwiseAnswerResponse
    {
        public int QuestionID { get; set; }
        public bool IsAnswerCorrect { get; set; }
        public string Explanation {  get; set; }
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
    public class QuestionAnalyticsResponseDTO
    {
        public decimal YourAccuracy { get; set; }
        public int YourTimeSpent { get; set; } // seconds
        public string YourTimeFormatted => ConvertSecondsToTimeFormat(YourTimeSpent);

        public decimal ClassmateAverageAccuracy { get; set; }
        public int ClassmateAverageTimeSpent { get; set; } // seconds
        public string ClassmateTimeFormatted => ConvertSecondsToTimeFormat(ClassmateAverageTimeSpent);

        public int TotalCorrectClassmates { get; set; }
        public int TotalClassmatesAttempted { get; set; }

        private string ConvertSecondsToTimeFormat(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.Hours > 0)
                return $"{time.Hours} hours {time.Minutes} minutes {time.Seconds} seconds";
            else if (time.Minutes > 0)
                return $"{time.Minutes} minutes {time.Seconds} seconds";
            else
                return $"{time.Seconds} seconds";
        }
    }

    public class PracticePerformanceStatsDto
    {
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int UnattemptedQuestions { get; set; }
        public decimal AverageAccuracyRate { get; set; }

        public decimal CorrectPercentage { get; set; }
        public decimal IncorrectPercentage { get; set; }
        public decimal UnattemptedPercentage { get; set; }
    }

    public class ChapterAccuracyReportResponse
    {
        public decimal MyAccuracyPercentage { get; set; }
        public decimal ClassmatesAccuracyPercentage { get; set; }
        public int StudentsWithBetterAccuracy { get; set; }
        public int TotalStudentsAttempted { get; set; }
        public int TotalAttemptsOfChapter { get; set; }
    }

    public class ChapterAnalyticsResponse
    {
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public int UnattemptedCount { get; set; }
        public decimal CorrectPercentage { get; set; }
        public decimal IncorrectPercentage { get; set; }
        public decimal UnattemptedPercentage { get; set; }
    }
    public class ChapterTimeReportResponse
    {
        public string TotalTimeSpentByMe { get; set; }
        public string AvgTimeSpentByMePerQuestion { get; set; }

        public string TotalTimeCorrect { get; set; }
        public string AvgTimeCorrect { get; set; }

        public string TotalTimeIncorrect { get; set; }
        public string AvgTimeIncorrect { get; set; }

        public string TotalTimeUnattempted { get; set; }
        public string AvgTimeUnattempted { get; set; }

        public string AvgTimeSpentByClassmates { get; set; }
        public string AvgTimeSpentByClassmatesPerQuestion { get; set; }
    }

    public class StudentTimeAnalysisDto
    {
        public string TotalTimeSpent { get; set; }
        public string AverageTimePerQuestion { get; set; }

        public string TotalTimeOnCorrectAnswers { get; set; }
        public string AverageTimePerCorrectAnswer { get; set; }

        public string TotalTimeOnIncorrectAnswers { get; set; }
        public string AverageTimePerIncorrectAnswer { get; set; }

        public string TotalTimeOnUnansweredQuestions { get; set; }
        public string AverageTimePerUnansweredQuestion { get; set; }
    }
    public class ChapterTreeResponse
    {
        public int SubjectId { get; set; }
        public int SyllabusId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentId { get; set; }
        public string ContentName { get; set; }
        public string Synopsis { get; set; }
        public int RegistrationId { get; set; }
        public decimal Percentage { get; set; }
        public int Question { get; set; }
        public int TopicCount { get; set; }
        public List<TopicResponse> Topics { get; set; }
    }

    public class TopicResponse
    {
        public string TopicName { get; set; }
        public int ContentId { get; set; }
        public int IndexTypeId { get; set; }
        public int SubTopicCount { get; set; }
        public decimal Percentage { get; set; }
        public int Question { get; set; }
        public List<SubTopicResponse> SubTopics { get; set; }
    }

    public class SubTopicResponse
    {
        public int ContentId { get; set; }
        public string SubTopicName { get; set; }
        public decimal Percentage { get; set; }
        public int Question { get; set; }
    }

}