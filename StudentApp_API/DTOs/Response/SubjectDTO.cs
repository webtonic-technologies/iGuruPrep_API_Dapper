namespace StudentApp_API.DTOs.Response
{

    public class SubjectDTO
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int ConceptCount {  get; set; }
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
        public bool IsChallengeApplicable { get; set; }
    }

}
