using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblSOTD")]
    public class StoryOfTheDay
    {
        [Key]
        public int QuestionId { get; set; }

        [StringLength(150)]
        public string Question { get; set; }

        [StringLength(15)]
        public string BoardName { get; set; }

        [StringLength(5)]
        public string ClassName { get; set; }

        public DateTime? PostTime { get; set; }

        public DateTime? DateAndTime { get; set; }

        [StringLength(50)]
        public string UploadImage { get; set; }

        [StringLength(5)]
        public string Answer { get; set; }

        public TimeSpan? AnswerRevealTime { get; set; }

        public int Status { get; set; }
    }
}
