using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblMagazine")]
    public class Magazine
    {
        public int MagazineId { get; set; }
        [MaxLength(255)]
        public string MagazineName { get; set; }
        [MaxLength(50)]
        public string ClassName { get; set; }
        [MaxLength(50)]
        public string CourseName { get; set; }
        public DateTime DateAndTime { get; set; }
        public string PathURL { get; set; }
    }
}
