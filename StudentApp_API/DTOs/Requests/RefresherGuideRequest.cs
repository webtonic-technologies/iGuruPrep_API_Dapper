namespace StudentApp_API.DTOs.Requests
{
    public class RefresherGuideRequest
    {
     public int? RegistrationId { get; set; }
    }
    public class GetContentRequest
    {
        public int SyllabusId { get; set; }
        public int SubjectId {  get; set; }
    }
    public class GetQuestionRequest
    {
        public int SubjectId { get; set; }        // The Subject ID to filter questions
        public int IndexTypeId { get; set; }      // Index Type (e.g., Chapter, Topic, Sub-Topic)
        public int ContentIndexId { get; set; }   // Content Index ID for filtering questions
    }
    public class SaveQuestionRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode {  get; set; }= string.Empty;
        public int StudentId { get; set; }
    }
    public class SyllabusDetailsRequest
    {

        public int? SyllabusId { get; set; }
        public int? SubjectId { get; set; }        // The Subject ID to filter questions
        public int? IndexTypeId { get; set; }      // Index Type (e.g., Chapter, Topic, Sub-Topic)
        public int? ContentIndexId { get; set; }   // Content Index ID for filtering questions
    }
}
