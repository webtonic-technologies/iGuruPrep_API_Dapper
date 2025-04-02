namespace StudentApp_API.DTOs.Response
{

    public class SubjectDTO
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int ChapterCount {  get; set; }
    }

    public class ChapterDTO
    {
        public int ChapterId { get; set; }
        public string ChapterName { get; set; }
        public string ChapterCode { get; set; }
        public int DisplayOrder { get; set; }
        public int ConceptCount { get; set; }
    }
    public class CYOTResponse
    {
        public int CYOTID { get; set; }
        public string CYOTName { get; set; }
        public int TotalQuestions { get; set; }
        public string Duration { get; set; }
        public int CYOTStatusID { get; set; }
        public string CYOTStatus { get; set; }
        public int Percentage { get; set; }
        public bool IsViewKey { get; set; } = false;
        public bool IsAnalytics { get; set; } = false;
        public bool IsChallengeApplicable { get; set; } = false;
        public DateTime? CreatedOn { get; set; }
    }
    public class CYOTMyChallengesAnalyticsResponse
    {
        public decimal AchievedMarks { get; set; }
        public decimal NegativeMarks { get; set; }
        public decimal FinalMarks { get; set; }
        public decimal FinalPercentage { get; set; }
        public decimal Percentile { get; set; }
        public int StudentsAboveMe { get; set; }
        public int TotalStudentsAttempted { get; set; }
        public int CountryRank { get; set; }
        public int NationalRank { get; set; }
    }
    public class CYOTMyChallengesTimeAnalytics
    {
        // Student's time data
        public string TotalTimeSpent { get; set; }
        public string AvgTimePerQuestion { get; set; }

        public string TotalTimeSpentCorrect { get; set; }
        public string AvgTimeSpentCorrect { get; set; }

        public string TotalTimeSpentWrong { get; set; }
        public string AvgTimeSpentWrong { get; set; }

        public string TotalTimeSpentUnattempted { get; set; }
        public string AvgTimeSpentUnattempted { get; set; }

        // Other students' average time data
        public string AvgTimeSpentByOthers { get; set; }
        public string AvgTimePerQuestionByOthers { get; set; }

        public string AvgTimeSpentCorrectByOthers { get; set; }
        public string AvgAvgTimeSpentCorrectByOthers { get; set; }

        public string AvgTimeSpentWrongByOthers { get; set; }
        public string AvgAvgTimeSpentWrongByOthers { get; set; }

        public string AvgTimeSpentUnattemptedByOthers { get; set; }
        public string AvgAvgTimeSpentUnattemptedByOthers { get; set; }
    }
    public class MarksComparison
    {
        public decimal MarksByMe { get; set; }
        public decimal MarksByTopper { get; set; }
        public decimal AvgMarksByOthers { get; set; }
    }
    public class LeaderboardResponse
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TotalScore { get; set; }
    }
}
