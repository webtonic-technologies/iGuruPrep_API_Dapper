using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_API.Models
{
    [Table("tblBook")]
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public int BoardID { get; set; }
        public int ClassID {  get; set; }
        public int CourseID { get; set; }
        public int SubjectID { get; set; }
        public bool Status {  get; set; }
        public string Authorname {  get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string AudioOrVideo { get; set; } = string.Empty;
        public int examtypeid { get; set; }
        public int APID { get; set; }
        public string boardname { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
        public string coursename { get; set; } = string.Empty;
        public string subjectname { get; set; } = string.Empty;
        public string APname { get; set; } = string.Empty;
        public DateTime? modifiedon {  get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public int FileTypeId { get; set; }
    }
}