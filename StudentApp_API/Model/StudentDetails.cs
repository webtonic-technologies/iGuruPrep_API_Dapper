namespace StudentApp_API.Models
{
    public class StudentDetails
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public int BoardId { get; set; }
    }

    public class SyllabusDetails
    {
        public int SyllabusId { get; set; }
        public int BoardID { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public string SyllabusName { get; set; }
    }
    public class CYOTSubjectMapping
    {
        public int CYOTId { get; set; }
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
    }
    public class QuizooSubjectMapping
    {
        public int QuizooID { get; set; }
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
    }

    public class ContentDetails
    {
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int ChapterContentIndexId { get; set; }
        public string ContentName {  get; set; }
        public int TopicContentIndexId { get; set; }
        public int SubTopicContentIndexId { get; set; }
        public int SubjectId { get; set; }
        public string ChapterName { get; set; }
        public string TopicName { get; set; }
        public string SubTopicName { get; set; }
        public int ChapterIndexTypeId { get; set; }
        public int TopicIndexTypeId { get; set; }
        public int SubTopicIndexTypeId { get; set; }
    }

    public class AnswerMaster
    {
        public int Answerid { get; set; }
        public int Questionid { get; set; }
        public int QuestionTypeid { get; set; }
    }
}
