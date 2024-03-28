using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_API.Models
{
    [Table("tblBook")]
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [StringLength(150)]
        public string BookName { get; set; }

        [StringLength(15)]
        public string AuthorName { get; set; }

        [StringLength(15)]
        public string AuthorDetails { get; set; }

        [StringLength(15)]
        public string AuthorAffliation { get; set; }

        [Required]
        [StringLength(5)]
        public string Boardname { get; set; }

        [Required]
        [StringLength(15)]
        public string ClassName { get; set; }

        [Required]
        [StringLength(15)]
        public string CourseName { get; set; }

        [StringLength(15)]
        public string SubjectName { get; set; }

        [Required]
        public int Status { get; set; }
    }
}