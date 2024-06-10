using System.ComponentModel.DataAnnotations;

namespace Course_API.Models
{
    public class TestSeriesSubjects
    {
        public int TestSeriesSubjectId { get; set; }
        [Required(ErrorMessage = "Subject name cannot be empty")]
        public int SubjectID { get; set; }
        public int TestSeriesID { get; set; }
        [Required(ErrorMessage = "Subject name cannot be empty")]
        public int NoOfQuestions { get; set; }
    }
}
