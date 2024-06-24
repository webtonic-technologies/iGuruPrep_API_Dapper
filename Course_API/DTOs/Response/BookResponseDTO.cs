namespace Course_API.DTOs.Response
{
    public class BookResponseDTO
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public bool Status { get; set; }
        public string pathURL { get; set; } = string.Empty;
        public string link { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public int FileTypeId { get; set; }
        public string FileTypeName { get; set; } = string.Empty;
        public List<BookAuthorDetailResponse>? BookAuthorDetails { get; set; }
        public List<BookCategoryResponse>? BookCategories { get; set; }
        public List<BookBoardResponse>? BookBoards { get; set; }
        public List<BookClassResponse>? BookClasses { get; set; }
        public List<BookCourseResponse>? BookCourses { get; set; }
        public List<BookExamTypeResponse>? BookExamTypes { get; set; }
        public List<BookSubjectResponse>? BookSubjects { get; set; }
    }
    public class BookAuthorDetailResponse
    {
        public int LibAuthDetailsId { get; set; }
        public int BookId { get; set; }
        public string AuthorDetails { get; set; } = string.Empty;
    }
    public class BookCategoryResponse
    {
        public int LibAuthCategoryId { get; set; }
        public int BookId { get; set; }
        public int APId { get; set; }
        public string APName { get; set; } = string.Empty;
    }
    public class BookBoardResponse
    {
        public int libraryBoardID { get; set; }
        public int bookID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class BookClassResponse
    {
        public int libraryClassID { get; set; }
        public int bookID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class BookCourseResponse
    {
        public int libraryCourseID { get; set; }
        public int bookID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class BookExamTypeResponse
    {
        public int libraryExamTypeID { get; set; }
        public int bookID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class BookSubjectResponse
    {
        public int librarySubjectID { get; set; }
        public int bookID { get; set; }
        public int SubjectID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
