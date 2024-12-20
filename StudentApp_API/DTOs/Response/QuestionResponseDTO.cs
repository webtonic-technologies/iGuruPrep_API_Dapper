namespace StudentApp_API.DTOs.Response
{
    public class QuestionResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int? QuestionTypeId { get; set; }
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
        public List<MatchPair>? MatchPairs { get; set; }
        public List<MatchThePairAnswer>? MatchThePairType2Answers { get; set; }
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public int DurationperQuestion {  get; set; }
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

    //public class QuestionResponse
    //{
    //    public int QuestionID { get; set; }
    //    public int AnswerID { get; set; }
    //    public int AnswerCount { get; set; }
    //    public double AnswerPercentage { get; set; }
    //}
    public class AnswerPercentageResponse
    {
        public int QuizID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public int AnswerCount { get; set; }
        public double AnswerPercentage { get; set; }
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
        public int Answerid { get; set; }
        public string Answer { get; set; }
    }

    // DTO class for the result
    public class StudentRankDTO
    {
        public int StudentID { get; set; }
        public int CorrectAnswers { get; set; }
        public int Rank { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
