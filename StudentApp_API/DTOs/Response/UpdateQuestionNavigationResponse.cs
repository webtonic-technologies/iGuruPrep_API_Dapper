namespace StudentApp_API.DTOs.Responses
{
    public class UpdateQuestionNavigationResponse
    {
        public int NavigationID { get; set; }
        public int ScholarshipID { get; set; }
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }
    }
    public class ScholarshipTestResponse
    {
        public ScholarshipTest ScholarshipTest { get; set; }
        public List<ScholarshipTestInstruction> Instructions { get; set; }
    }

    public class ScholarshipTest
    {
        public int ScholarshipTestId { get; set; }
        public int APID { get; set; }
        public int ExamTypeId { get; set; }
        public string PatternName { get; set; }
        public int TotalNumberOfQuestions { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int EmployeeID { get; set; }
        public int TotalMarks {  get; set; }
        public string Discount {  get; set; }
    }

    public class ScholarshipTestInstruction
    {
        public int SSTInstructionsId { get; set; }
        public string Instructions { get; set; }
        public int ScholarshipTestId { get; set; }
        public string InstructionName { get; set; }
        public int InstructionId { get; set; }
    }

    public class StudentClassCourseMappings
    {
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
    }
    public class SubjectQuestionCountResponse
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int TotalQuestions { get; set; }
    }
    public class QuestionResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public string QuestionTypeName {  get; set; }
        public bool? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int subjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public bool? IsRejected { get; set; }
        public bool? IsApproved { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string userRole { get; set; } = string.Empty;
        // New Properties
        public string StudentAnswer { get; set; } = string.Empty; // Submitted answer
        public bool? IsCorrect { get; set; } // True if correct, false otherwise
        public List<AnswerMultipleChoiceCategory>? AnswerMultipleChoiceCategories { get; set; }
        public Answersingleanswercategory? Answersingleanswercategories { get; set; }
    }

    public class AnswerMultipleChoiceCategory
    {
        public int Answermultiplechoicecategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
        public bool Iscorrect { get; set; }
        public int? Matchid { get; set; }
    }

    public class Answersingleanswercategory
    {
        public int Answersingleanswercategoryid { get; set; }
        public int? Answerid { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    public class SectionSettingDTO
    {
        public int SSTSectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public int TotalNumberOfQuestions { get; set; }
        public int SubjectId { get; set; }
    }
    public class MarksAcquiredAfterAnswerSubmission
    {
        public int RegistrationId { get; set; }
        public int ScholarshipId {  get; set; }
        public int QuestionId {  get; set; }
        public decimal MarksAcquired {  get; set; }
    }
    public class QuestionTypeResponse
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; }
    }
    public class ScholarshipSectionResponse
    {
        public int SSTSectionId { get; set; }
        public int ScholarshipTestId { get; set; }
        public string SectionName { get; set; }
        public int QuestionTypeId { get; set; }
    }
    public class StudentDiscountResponse
    {
        public decimal TotalMarksGained { get; set; }
        public decimal TotalMarksPossible { get; set; }
        public decimal Percentage { get; set; }
        public decimal Discount { get; set; }
    }
}
