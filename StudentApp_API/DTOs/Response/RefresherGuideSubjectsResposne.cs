namespace StudentApp_API.DTOs.Response
{
    public class RefresherGuideSubjectsResposne
    {
        public int SyllabusId {  get; set; }
        public int SubjectId {  get; set; }
        public string SubjectName {  get; set; }
        public int? RegistrationId {  get; set; }
        public decimal Percentage {  get; set; }
    }
    public class RefresherGuideSubjects
    {
        public List<RefresherGuideSubjectsResposne>? refresherGuideSubjectsResposnes {  get; set; }
        public decimal Percentage { get; set; }
    }
    public class RefresherGuideContentResponse
    {
        public int SubjectId { get; set; }
        public int SyllabusId { get; set; }
        public int IndexTypeId {  get; set; }
        public int ContentId {  get; set; }
        public string ContentName { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public decimal Percentage {  get; set; }
    }
    public class QuestionResponse
    {
        public int QuestionId { get; set; }                   // Unique ID of the question
        public string QuestionDescription { get; set; }       // Text of the question
        public string QuestionFormula { get; set; }           // Optional formula related to the question
        public string QuestionImage { get; set; }             // URL or path to the question image (if any)
        public int DifficultyLevelId { get; set; }            // Difficulty level (easy, medium, hard)
        public int QuestionTypeId { get; set; }               // Type of question (SA, LA, VSA)
        public int IndexTypeId { get; set; }                    // Time allocated for the question
        public string Explanation { get; set; }               // Explanation or solution for the question
        public bool IsActive { get; set; }                    // Is the question active
        public bool IsLive { get; set; }                      // Is the question live
        public AnswerResponse Answers { get; set; }     // List of associated answers for the question
    }

    public class AnswerResponse
    {
        public int AnswerId { get; set; }                     // Unique ID of the answer
        public string Answer { get; set; }                    // Answer text for single-answer questions
        public string QuestionCode { get; set; }              // Code associated with the question
    }
    public class PercentageResponse
    {
        public int SubjectId { get; set; }
        public int ChapterId { get; set; }
        public int TopicId { get; set; }
        public int SubTopicId { get; set; }
        public string SubTopicName { get; set; }
        public decimal Percentage { get; set; }
    }

    public class QuestionData
    {
        public int SubjectId { get; set; }
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public int IsRead { get; set; } // or boolean if applicable
        public string ContentName_Chapter { get; set; }
        public string ContentName_Topic { get; set; }
        public string ContentName_SubTopic { get; set; }
    }
}
