namespace StudentApp_API.DTOs.Responses
{
    public class ScholarshipQuestionsResponse
    {
        public int ScholarshipId { get; set; }
        public List<ScholarshipSubjects>? ScholarshipSubjects { get; set; }
    }
    public class ScholarshipSubjects
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public List<ScholarshipSections>? ScholarshipSections { get; set; }
    }
    public class ScholarshipSections
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int QuestionTypeId {  get; set; }
        public List<QuestionResponseDTO>? QuestionResponseDTOs {  get; set; }
    }
    public class ScholarshipViewKeyQuestionsResponse
    {
        public int ScholarshipId { get; set; }
        public List<ScholarshipViewKeySubjects>? ScholarshipSubjects { get; set; }
    }
    public class ScholarshipViewKeySubjects
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public List<ScholarshipViewKeySections>? ScholarshipSections { get; set; }
    }
    public class ScholarshipViewKeySections
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int QuestionTypeId { get; set; }
        public List<QuestionViewKeyResponseDTO>? QuestionResponseDTOs { get; set; }
    }
}