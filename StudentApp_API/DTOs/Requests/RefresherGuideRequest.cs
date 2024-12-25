namespace StudentApp_API.DTOs.Requests
{
    public class GetTestseriesSubjects 
    { 
        public int SubjectId {  get; set; }
        public int RegistrationId {  get; set; }
        public int PageNumber { get; set; }
        public int PageSize {  get; set; }
    }

    public class RefresherGuideRequest
    {
     public int RegistrationId { get; set; }
    }
    public class GetQuestionRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SubjectId { get; set; }        // The Subject ID to filter questions
        public int IndexTypeId { get; set; }      // Index Type (e.g., Chapter, Topic, Sub-Topic)
        public int ContentIndexId { get; set; }   // Content Index ID for filtering questions
        public List<int>? QuestionTypeId { get; set; }
    }
    public class SaveQuestionRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode {  get; set; }= string.Empty;
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
        public int TestSeriesId {  get; set; }
    }
    public class SaveQuestionRefresherGuidwRequest
    {
        public int QuestionId { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public int SubjectId { get; set; }
      //  public int TestSeriesId { get; set; }
    }
    public class SyllabusDetailsRequest
    {

        public int? SyllabusId { get; set; }
        public int? SubjectId { get; set; }        // The Subject ID to filter questions
        public int? IndexTypeId { get; set; }      // Index Type (e.g., Chapter, Topic, Sub-Topic)
        public int? ContentIndexId { get; set; } // Content Index ID for filtering questions
        public int RegistrationId { get; set; }
    }
}
