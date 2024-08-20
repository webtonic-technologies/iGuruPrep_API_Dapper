using System.ComponentModel.DataAnnotations;

namespace Course_API.DTOs.Requests
{
    public class BookDTO
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public bool Status { get; set; }
        [Required(ErrorMessage = "url cannot be empty")]
        public string Image { get; set; } = string.Empty;
        [Required(ErrorMessage = "file cannot be empty")]
        public string AudioOrVideo { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "File Type cannot be empty")]
        public int FileTypeId { get; set; }
        public List<BookAuthorDetail>? BookAuthorDetails { get; set; }
        public List<BookCategory>? BookCategories { get; set; }
        public List<BookBoard>? BookBoards { get; set; }
        public List<BookClass>? BookClasses { get; set; }
        public List<BookCourse>? BookCourses { get; set; }
        public List<BookExamType>? BookExamTypes { get; set; }
        public List<BookSubject>? BookSubjects { get; set; }
    }
    public class BookAuthorDetail
    {
        public int LibAuthDetailsId { get; set; }
        public int BookId { get; set; }
        [Required(ErrorMessage = "Author details cannot be empty")]
        public string AuthorDetails { get; set; } = string.Empty;
    }
    public class BookCategory
    {
        public int LibAuthCategoryId { get; set; }
        public int BookId { get; set; }
        public int APId { get; set; }
        // public string APName { get; set; } = string.Empty;
    }
    public class BookBoard
    {
        public int libraryBoardID { get; set; }
        public int bookID { get; set; }
        public int BoardID { get; set; }
    }
    public class BookClass
    {
        public int libraryClassID { get; set; }
        public int bookID { get; set; }
        public int ClassID { get; set; }
    }
    public class BookCourse
    {
        public int libraryCourseID { get; set; }
        public int bookID { get; set; }
        public int CourseID { get; set; }
    }
    public class BookExamType
    {
        public int libraryExamTypeID { get; set; }
        public int bookID { get; set; }
        public int ExamTypeID { get; set; }
    }
    public class BookSubject
    {
        public int librarySubjectID { get; set; }
        public int bookID { get; set; }
        [Required(ErrorMessage = "Subject name cannot be empty")]
        public int SubjectID { get; set; }
    }
    public class BookListDTO
    {
        public int APId { get; set; }
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int SubjectID { get; set; }
        public int ExamTypeID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
