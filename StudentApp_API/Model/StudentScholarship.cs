namespace StudentApp_API.Models
{
    public class StudentScholarship
    {
        public int SSID { get; set; }
        public int ScholarshipID { get; set; }
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }
        public int QuestionTypeID { get; set; }
        public int SrNo { get; set; }
        public DateTime ExamDate { get; set; }
    }
}
