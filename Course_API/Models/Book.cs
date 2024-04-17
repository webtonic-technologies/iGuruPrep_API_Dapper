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
        public string BookName { get; set; } = string.Empty;

        [StringLength(15)]
        public string AuthorName { get; set; } = string.Empty;

        [StringLength(15)]
        public string AuthorDetails { get; set; } = string.Empty;

        [StringLength(15)]
        public string AuthorAffliation { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string Boardname { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(15)]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        public int Status { get; set; }
    }
}