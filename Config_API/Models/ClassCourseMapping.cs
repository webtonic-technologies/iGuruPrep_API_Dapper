using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class ClassCourseMapping
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseClassMappingID { get; set; }
        public int? CourseID { get; set; }
        public int? ClassID { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
