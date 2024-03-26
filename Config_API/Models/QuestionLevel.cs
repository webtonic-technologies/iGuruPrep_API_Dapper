using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class QuestionLevel
    {
        //public int QuestionLevelId { get; set; }
        //public string QuestionLevelName { get; set;} = string.Empty;
        //public string QuestionLevelCode { get; set; } = string.Empty;
        //public int? NoOfQuestions { get; set;}
        //public int? SuccessRate { get;set; }
        //public bool? Status { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelCode { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string PatternCode { get; set; } = string.Empty;
    }
}
