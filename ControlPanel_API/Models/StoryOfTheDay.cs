using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblSOTD")]
    public class StoryOfTheDay
    {
        [Key]
        public int? StoryId {  get; set; }
        public int Questionid { get; set; }

        [StringLength(150)]
        public string QuestionName { get; set; } = string.Empty;
        public string BoardID { get; set; } = string.Empty;
        public string ClassID { get; set; } = string.Empty;
        [StringLength(15)]
        public string BoardName { get; set; } = string.Empty;

        [StringLength(5)]
        public string ClassName { get; set; } = string.Empty;
        public DateTime? PostTime { get; set; }

        public DateTime? DateAndTime { get; set; }

        [StringLength(50)]
        public string UploadImage { get; set; } = string.Empty;

        [StringLength(5)]
        public string Answer { get; set; } = string.Empty;

        public TimeSpan? AnswerRevealTime { get; set; }

        public int Status { get; set; }
    }
}
