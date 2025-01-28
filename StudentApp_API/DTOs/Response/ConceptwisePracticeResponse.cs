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
}