namespace StudentApp_API.Models
{
    public class QuestionNavigation
    {
        public int NavigationID { get; set; }
        public int QuestionID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ScholarshipID { get; set; }
        public int StudentID { get; set; }
    }
}
