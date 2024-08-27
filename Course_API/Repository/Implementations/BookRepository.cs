using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string? _connectionString;

        public BookRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> AddUpdate(BookDTO request)
        {

            try
            {
                if (request.BookId == 0)
                {
                    var book = new Book
                    {
                        BookName = request.BookName,
                        Status = true,
                        Image = ImageUpload(request.Image),
                        AudioOrVideo = AudioVideoUpload(request.AudioOrVideo),
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        EmployeeID = request.EmployeeID,
                        FileTypeId = request.FileTypeId
                    };
                    string insertQuery = @"
        INSERT INTO [tblLibrary] 
            (BookName, Status, Image, AudioOrVideo, createdon, createdby, EmployeeID, FileTypeId)
        VALUES 
            (@BookName, @Status, @Image, @AudioOrVideo, @createdon, @createdby, @EmployeeID, @FileTypeId);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    int insertedId = await _connection.QueryFirstOrDefaultAsync<int>(insertQuery, book);
                    if (insertedId > 0)
                    {
                        int category = BookCategoryMapping(request.BookCategories ??= ([]), insertedId);
                        int classes = BookClassMapping(request.BookClasses ??= ([]), insertedId);
                        int board = BookBoardMapping(request.BookBoards ??= ([]), insertedId);
                        int course = BookCourseMapping(request.BookCourses ??= ([]), insertedId);
                        int exam = BookExamTypeMapping(request.BookExamTypes ??= ([]), insertedId);
                        int subject = BookSubjectMapping(request.BookSubjects ??= ([]), insertedId);
                        int author = BookAuthorMapping(request.BookAuthorDetails ??= ([]), insertedId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0 && author > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Book Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }

                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var book = new Book
                    {
                        BookName = request.BookName,
                        Status = true,
                        Image = ImageUpload(request.Image),
                        AudioOrVideo = AudioVideoUpload(request.AudioOrVideo),
                        modifiedon = DateTime.Now,
                        modifiedby = request.modifiedby,
                        EmployeeID = request.EmployeeID,
                        FileTypeId = request.FileTypeId,
                        BookId = request.BookId
                    };
                    string updateQuery = @"
        UPDATE [tblLibrary]
        SET 
            BookName = @BookName,
            Status = @Status,
            Image = @Image,
            AudioOrVideo = @AudioOrVideo,
            modifiedon = @modifiedon,
            modifiedby = @modifiedby,
            EmployeeID = @EmployeeID,
            FileTypeId = @FileTypeId
        WHERE
            BookId = @BookId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, book);
                    if (rowsAffected > 0)
                    {
                        int category = BookCategoryMapping(request.BookCategories ??= ([]), request.BookId);
                        int classes = BookClassMapping(request.BookClasses ??= ([]), request.BookId);
                        int board = BookBoardMapping(request.BookBoards ??= ([]), request.BookId);
                        int course = BookCourseMapping(request.BookCourses ??= ([]), request.BookId);
                        int exam = BookExamTypeMapping(request.BookExamTypes ??= ([]), request.BookId);
                        int subject = BookSubjectMapping(request.BookSubjects ??= ([]), request.BookId);
                        int author = BookAuthorMapping(request.BookAuthorDetails ??= ([]), request.BookId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0 && author > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Book Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<bool>> Delete(int id)
        {
            try
            {
                var book = await _connection.QueryFirstOrDefaultAsync<Book>(
                   "SELECT * FROM tblBook WHERE BookId = @BookId",
                   new { BookId = id });

                if (book == null)
                    throw new Exception("Book not found");
                
                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo", book.Image);
                var filePath1 = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo", book.AudioOrVideo);
                if (File.Exists(filePath) || File.Exists(filePath1))
                {
                    File.Delete(filePath1);
                    File.Delete(filePath);
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "DELETE FROM tblBook WHERE BookId = @BookId", new { BookId = id });

                if (rowsAffected > 0)
                {
                    var deleteCat = @"DELETE FROM [tbllibraryCategory]
                          WHERE BookId = @BookId;";
                    var delCat = _connection.Execute(deleteCat, new { BookId = id });

                    var deleteClas = @"DELETE FROM [tbllibraryClass]
                          WHERE bookID = @BookId;";
                    var delClass = _connection.Execute(deleteClas, new { BookId = id });

                    var deleteBoard = @"DELETE FROM [tbllibraryBoard]
                          WHERE bookID = @BookId;";
                    var delBoard = _connection.Execute(deleteBoard, new { BookId = id });

                    var deleteCourse = @"DELETE FROM [tbllibraryCourse]
                          WHERE bookID = @BookId;";
                    var delCourse = _connection.Execute(deleteCourse, new { BookId = id });

                    var deleteExamType = @"DELETE FROM [tbllibraryExamType]
                          WHERE bookID = @BookId;";
                    var delExamType = _connection.Execute(deleteExamType, new { BookId = id });

                    var deleteSubject = @"DELETE FROM [tbllibrarySubject]
                          WHERE bookID = @BookId;";
                    var delSubject = _connection.Execute(deleteSubject, new { BookId = id });

                    return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        public async Task<ServiceResponse<BookResponseDTO>> Get(int id)
        {
            try
            {
                var response = new BookResponseDTO();

                string selectQuery = @"
                SELECT 
                    l.BookId,
                    l.BookName,
                    l.Status,
                    l.Image,
                    l.AudioOrVideo,
                    l.modifiedon,
                    l.modifiedby,
                    l.createdon,
                    l.createdby,
                    l.EmployeeID,
                    e.EmpFirstName,
                    l.FileTypeId,
                    f.FileType AS FileTypeName
                FROM 
                    [tblLibrary] l
                LEFT JOIN 
                    [tblEmployee] e ON l.EmployeeID = e.Employeeid
                LEFT JOIN 
                    [tblLibraryFileType] f ON l.FileTypeId = f.tblLibraryFileType
                WHERE 
                    l.BookId = @BookId";

                var book = await _connection.QueryFirstOrDefaultAsync<dynamic>(selectQuery, new { BookId = id });
                if (book != null)
                {
                    response.BookId = book.BookId;
                    response.BookName = book.BookName;
                    response.Status = book.Status;
                    response.Image = GetImage(book.Image);
                    response.AudioOrVideo = GetAudioVideo(book.AudioOrVideo);
                    response.modifiedon = book.modifiedon;
                    response.modifiedby = book.modifiedby;
                    response.createdon = book.createdon;
                    response.createdby = book.createdby;
                    response.EmployeeID = book.EmployeeID;
                    response.EmpFirstName = book.EmpFirstName;
                    response.FileTypeId = book.FileTypeId;
                    response.FileTypeName = book.FileTypeName;
                    response.BookAuthorDetails = GetListOfAuthorDetails(book.BookId);
                    response.BookSubjects = GetListOfBookSubject(book.BookId);
                    response.BookBoards = GetListOfBookBoards(book.BookId);
                    response.BookClasses = GetListOfBookClass(book.BookId);
                    response.BookCourses = GetListOfBookCourse(book.BookId);
                    response.BookExamTypes = GetListOfBookExamType(book.BookId);
                    response.BookCategories = GetListOfBookCategory(book.BookId);

                    return new ServiceResponse<BookResponseDTO>(true, "Record Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<BookResponseDTO>(false, "Record not Found", new BookResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<BookResponseDTO>(false, ex.Message, new BookResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<BookResponseDTO>>> GetAllBooks(BookListDTO request)
        {
            try
            {
                var employeeRoleQuery = "SELECT e.RoleID, r.RoleCode FROM tblEmployee e INNER JOIN tblRole r ON e.RoleID = r.RoleID WHERE e.Employeeid = @EmployeeID";
                var employeeRole = await _connection.QuerySingleOrDefaultAsync<dynamic>(employeeRoleQuery, new { EmployeeID = request.EmployeeId });

                // Determine if the employee is Admin or SuperAdmin
                bool isAdminOrSuperAdmin = employeeRole != null && (employeeRole.RoleCode == "AD" || employeeRole.RoleCode == "SA");
                // Base query
                string baseQuery = @"
        SELECT DISTINCT
            l.BookId,
            l.BookName,
            l.Status,
            l.Image,
            l.AudioOrVideo,
            l.modifiedon,
            l.modifiedby,
            l.createdon,
            l.createdby,
            l.EmployeeID,
            e.EmpFirstName,
            l.FileTypeId,
            f.FileType
        FROM [tblLibrary] l
        LEFT JOIN [tblEmployee] e ON l.EmployeeID = e.Employeeid
        LEFT JOIN [tblLibraryFileType] f ON l.FileTypeId = f.tblLibraryFileType
        LEFT JOIN [tbllibraryCategory] lc ON l.BookId = lc.BookId
        LEFT JOIN [tbllibraryBoard] lb ON l.BookId = lb.BookId
        LEFT JOIN [tbllibraryClass] lc2 ON l.BookId = lc2.BookId
        LEFT JOIN [tbllibraryCourse] lco ON l.BookId = lco.BookId
        LEFT JOIN [tbllibraryExamType] le ON l.BookId = le.BookId
        LEFT JOIN [tbllibrarySubject] ls ON l.BookId = ls.BookId
        WHERE 1=1";

                // Applying filters
                if (request.ClassID > 0)
                {
                    baseQuery += @"
    AND lc2.ClassID = @ClassID
    AND lc2.BookId IN (
        SELECT lc2.BookId 
        FROM [tbllibraryClass] lc2 
        INNER JOIN [tblClass] c ON lc2.ClassID = c.ClassID 
        WHERE c.Status = 1
    )";
                }

                if (request.BoardID > 0)
                {
                    baseQuery += @"
    AND lb.BoardID = @BoardID
    AND lb.BookId IN (
        SELECT lb.BookId 
        FROM [tbllibraryBoard] lb 
        INNER JOIN [tblBoard] b ON lb.BoardID = b.BoardID 
        WHERE b.Status = 1
    )";
                }

                if (request.CourseID > 0)
                {
                    baseQuery += @"
    AND lco.CourseID = @CourseID
    AND lco.BookId IN (
        SELECT lco.BookId 
        FROM [tbllibraryCourse] lco 
        INNER JOIN [tblCourse] co ON lco.CourseID = co.CourseID 
        WHERE co.Status = 1
    )";
                }

                if (request.SubjectID > 0)
                {
                    baseQuery += @"
    AND ls.SubjectID = @SubjectID
    AND ls.BookId IN (
        SELECT ls.BookId 
        FROM [tbllibrarySubject] ls 
        INNER JOIN [tblSubject] s ON ls.SubjectID = s.SubjectId 
        WHERE s.Status = 1
    )";
                }
                //if (request.ClassID > 0)
                //{
                //    baseQuery += " AND lc2.ClassID = @ClassID";
                //}
                //if (request.BoardID > 0)
                //{
                //    baseQuery += " AND lb.BoardID = @BoardID";
                //}
                //if (request.CourseID > 0)
                //{
                //    baseQuery += " AND lco.CourseID = @CourseID";
                //}
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND le.ExamTypeID = @ExamTypeID";
                }
                if (request.APId > 0)
                {
                    baseQuery += " AND lc.APId = @APId";
                }
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery += " AND s.Status = 1";
                }
                //if (request.SubjectID > 0)
                //{
                //    baseQuery += " AND ls.SubjectID = @SubjectID";
                //}

                // Parameters for the query
                var parameters = new
                {
                    ClassID = request.ClassID,
                    BoardID = request.BoardID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APId = request.APId,
                    SubjectID = request.SubjectID
                };

                // Fetch all matching records
                var mainResult = (await _connection.QueryAsync<dynamic>(baseQuery, parameters)).ToList();

                // Total count before pagination
                int totalCount = mainResult.Count;

                // Map results to response DTO
                var response = mainResult.Select(item => new BookResponseDTO
                {
                    BookId = item.BookId,
                    BookName = item.BookName,
                    Status = item.Status,
                    Image = GetImage(item.Image),
                    AudioOrVideo = GetAudioVideo(item.AudioOrVideo),
                    modifiedon = item.modifiedon,
                    modifiedby = item.modifiedby,
                    createdon = item.createdon,
                    createdby = item.createdby,
                    EmployeeID = item.EmployeeID,
                    EmpFirstName = item.EmpFirstName,
                    FileTypeId = item.FileTypeId,
                    FileTypeName = item.FileType, // Ensure this property is correctly retrieved
                    BookAuthorDetails = GetListOfAuthorDetails(item.BookId),
                    BookCategories = GetListOfBookCategory(item.BookId),
                    BookBoards = GetListOfBookBoards(item.BookId),
                    BookClasses = GetListOfBookClass(item.BookId),
                    BookCourses = GetListOfBookCourse(item.BookId),
                    BookExamTypes = GetListOfBookExamType(item.BookId),
                    BookSubjects = GetListOfBookSubject(item.BookId)
                }).ToList();

                // Apply logical pagination
                var paginatedResponse = response
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if there are records
                if (paginatedResponse.Any())
                {
                    return new ServiceResponse<List<BookResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<BookResponseDTO>>(false, "Records not found", new List<BookResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<BookResponseDTO>>(false, ex.Message, new List<BookResponseDTO>(), 500);
            }
        }
        private int BookCategoryMapping(List<BookCategory> request, int BookId)
        {
            foreach (var data in request)
            {
                data.BookId = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryCategory] WHERE [BookId] = @BookId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryCategory]
                          WHERE [BookId] = @BookId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryCategory] ([APId], [BookId])
                          VALUES (@APId, @BookId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryCategory] ([APId], [BookId])
                          VALUES (@APId, @BookId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookClassMapping(List<BookClass> request, int BookId)
        {
            foreach (var data in request)
            {
                data.bookID = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryClass] WHERE [bookID] = @bookID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { bookID = BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryClass]
                          WHERE [bookID] = @bookID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { bookID = BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryClass] ([bookID], [ClassID])
                          VALUES (@bookID, @ClassID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryClass] ([bookID], [ClassID])
                          VALUES (@bookID, @ClassID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookBoardMapping(List<BookBoard> request, int BookId)
        {
            foreach (var data in request)
            {
                data.bookID = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryBoard] WHERE [bookID] = @bookID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { bookID = BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryBoard]
                          WHERE [bookID] = @bookID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { bookID = BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryBoard] ([bookID], [BoardID])
                          VALUES (@bookID, @BoardID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryBoard] ([bookID], [BoardID])
                          VALUES (@bookID, @BoardID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookCourseMapping(List<BookCourse> request, int BookId)
        {
            foreach (var data in request)
            {
                data.bookID = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryCourse] WHERE [bookID] = @bookID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { bookID = BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryCourse]
                          WHERE [bookID] = @bookID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { bookID = BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryCourse] ([bookID], [CourseID])
                          VALUES (@bookID, @CourseID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryCourse] ([bookID], [CourseID])
                          VALUES (@bookID, @CourseID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookExamTypeMapping(List<BookExamType> request, int BookId)
        {
            foreach (var data in request)
            {
                data.bookID = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryExamType] WHERE [bookID] = @bookID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { bookID = BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryExamType]
                          WHERE [bookID] = @bookID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { bookID = BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryExamType] ([bookID], [ExamTypeID])
                          VALUES (@bookID, @ExamTypeID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryExamType] ([bookID], [ExamTypeID])
                          VALUES (@bookID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookSubjectMapping(List<BookSubject> request, int BookId)
        {
            foreach (var data in request)
            {
                data.bookID = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibrarySubject] WHERE [bookID] = @bookID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { bookID = BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibrarySubject]
                          WHERE [bookID] = @bookID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { bookID = BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibrarySubject] ([bookID], [SubjectID])
                          VALUES (@bookID, @SubjectID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibrarySubject] ([bookID], [SubjectID])
                          VALUES (@bookID, @SubjectID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int BookAuthorMapping(List<BookAuthorDetail> request, int BookId)
        {
            foreach (var data in request)
            {
                data.BookId = BookId;
            }
            string query = "SELECT COUNT(*) FROM [tbllibraryAuthorDetails] WHERE [BookId] = @BookId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { BookId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbllibraryAuthorDetails]
                          WHERE [BookId] = @BookId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { BookId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tbllibraryAuthorDetails] ([BookId], [AuthorDetails])
                          VALUES (@BookId, @AuthorDetails);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tbllibraryAuthorDetails] ([BookId], [AuthorDetails])
                          VALUES (@BookId, @AuthorDetails);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<BookBoardResponse> GetListOfBookBoards(int BookId)
        {
            var boardquery = @"
    SELECT lb.*, b.BoardName as Name
    FROM [tbllibraryBoard] lb
    JOIN [tblBoard] b ON lb.BoardId = b.BoardId
    WHERE lb.BookID = @BookID
      AND b.Status = 1;"; // Check for active boards
            var data = _connection.Query<BookBoardResponse>(boardquery, new { BookID = BookId });
            return data != null ? data.AsList() : new List<BookBoardResponse>();
        }
        private List<BookCategoryResponse> GetListOfBookCategory(int BookId)
        {
            var query = @"
            SELECT lc.*, c.APName
            FROM [tbllibraryCategory] lc
            JOIN [tblCategory] c ON lc.APId = c.APID
            WHERE lc.BookId = @BookId;";
            var data = _connection.Query<BookCategoryResponse>(query, new { BookId });
            return data != null ? data.AsList() : [];
        }
        private List<BookClassResponse> GetListOfBookClass(int BookId)
        {
            var query = @"
    SELECT lcl.*, cl.ClassName as Name
    FROM [tbllibraryClass] lcl
    JOIN [tblClass] cl ON lcl.ClassId = cl.ClassId
    WHERE lcl.BookID = @BookID
      AND cl.Status = 1;"; // Check for active classes
            var data = _connection.Query<BookClassResponse>(query, new { BookID = BookId });
            return data != null ? data.AsList() : new List<BookClassResponse>();
        }
        private List<BookCourseResponse> GetListOfBookCourse(int BookId)
        {
            var query = @"
    SELECT lc.*, c.CourseName as Name
    FROM [tbllibraryCourse] lc
    JOIN [tblCourse] c ON lc.CourseId = c.CourseId
    WHERE lc.BookID = @BookID
      AND c.Status = 1;"; // Check for active courses
            var data = _connection.Query<BookCourseResponse>(query, new { BookID = BookId });
            return data != null ? data.AsList() : new List<BookCourseResponse>();
        }
        private List<BookExamTypeResponse> GetListOfBookExamType(int bookID)
        {
            var query = @"
            SELECT let.*, et.ExamTypeName as Name
            FROM [tbllibraryExamType] let
            JOIN [tblExamType] et ON let.ExamTypeId = et.ExamTypeId
            WHERE let.BookID = @bookID;";
            var data = _connection.Query<BookExamTypeResponse>(query, new { bookID });
            return data != null ? data.AsList() : [];
        }
        private List<BookSubjectResponse> GetListOfBookSubject(int BookId)
        {
            var query = @"
    SELECT ls.*, s.SubjectName as Name
    FROM [tbllibrarySubject] ls
    JOIN [tblSubject] s ON ls.SubjectId = s.SubjectId
    WHERE ls.BookID = @BookID
      AND s.Status = 1;"; // Check for active subjects
            var data = _connection.Query<BookSubjectResponse>(query, new { BookID = BookId });
            return data != null ? data.AsList() : new List<BookSubjectResponse>();
        }
        private List<BookAuthorDetailResponse> GetListOfAuthorDetails(int BookId)
        {
            var query = @"SELECT * FROM [tbllibraryAuthorDetails] WHERE  BookId = @BookId;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookAuthorDetailResponse>(query, new { BookId });
            return data != null ? data.AsList() : [];
        }
        private string ImageUpload(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Books");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(imageData) == true ? ".jpg" : IsPng(imageData) == true ? ".png" : IsGif(imageData) == true ? ".gif" : string.Empty;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return $"/Assets/Books/{fileName}";
        }
        private bool IsJpeg(byte[] bytes)
        {
            // JPEG magic number: 0xFF, 0xD8
            return bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xD8;
        }
        private bool IsPng(byte[] bytes)
        {
            // PNG magic number: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            return bytes.Length > 7 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }
        private bool IsGif(byte[] bytes)
        {
            // GIF magic number: "GIF"
            return bytes.Length > 2 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
        private string AudioVideoUpload(string data)
        {
            if (string.IsNullOrEmpty(data) || data == "string")
            {
                return string.Empty;
            }
            byte[] bytes = Convert.FromBase64String(data);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileExtension = IsMP3(bytes) == true ? ".mp3" : IsWAV(bytes) == true ? ".wav" :
                IsAAC(bytes) == true ? ".aac" : IsOGG(bytes) == true ? ".ogg" :
                IsFLAC(bytes) == true ? ".flac" : IsM4A(bytes) == true ? ".m4a" :
                IsMp4(bytes) == true ? ".mp4" : IsMov(bytes) == true ? ".mov" : IsAvi(bytes) == true ? ".avi" : string.Empty; ;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
        private bool IsMp4(byte[] bytes)
        {
            // MP4 magic number: 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70
            return bytes.Length > 7 &&
                   bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0x00 && bytes[3] == 0x20 &&
                   bytes[4] == 0x66 && bytes[5] == 0x74 && bytes[6] == 0x79 && bytes[7] == 0x70;
        }
        private bool IsAvi(byte[] bytes)
        {
            // AVI magic number: "RIFF"
            return bytes.Length > 3 &&
                   bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46;
        }
        private bool IsMov(byte[] bytes)
        {
            // MOV magic number: "moov"
            return bytes.Length > 3 &&
                   bytes[0] == 0x6D && bytes[1] == 0x6F && bytes[2] == 0x6F && bytes[3] == 0x76;
        }
        //extensions for audio
        private bool IsMP3(byte[] bytes)
        {
            // MP3 magic number: 0x49 0x44 0x33
          //  return bytes.Length > 2 && bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33;


            return bytes.Length > 1 &&
         (bytes[0] == 0xFF && (bytes[1] & 0xF0) == 0xF0 || // MPEG-1 Layer 3
          bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33); // ID3 tag
        }
        private bool IsWAV(byte[] bytes)
        {
            // WAV magic number: "RIFF"
            return bytes.Length > 3 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46;
        }
        private bool IsAAC(byte[] bytes)
        {
            // AAC magic number: "ADIF" or "ADTS"
            return bytes.Length > 3 && (bytes[0] == 0x41 && bytes[1] == 0x44 && bytes[2] == 0x49 && bytes[3] == 0x46) ||
                   (bytes[0] == 0xFF && (bytes[1] & 0xF0) == 0xF0 && (bytes[2] & 0xF0) == 0xF0 && (bytes[3] & 0xF0) == 0xF0);
        }
        private bool IsOGG(byte[] bytes)
        {
            // OGG magic number: "OggS"
            return bytes.Length > 3 && bytes[0] == 0x4F && bytes[1] == 0x67 && bytes[2] == 0x67 && bytes[3] == 0x53;
        }
        private bool IsFLAC(byte[] bytes)
        {
            // FLAC magic number: "fLaC"
            return bytes.Length > 3 && bytes[0] == 0x66 && bytes[1] == 0x4C && bytes[2] == 0x61 && bytes[3] == 0x43;
        }
        private bool IsM4A(byte[] bytes)
        {
            // M4A magic number: "ftyp"
            return bytes.Length > 3 && bytes[0] == 0x66 && bytes[1] == 0x74 && bytes[2] == 0x79 && bytes[3] == 0x70;
        }
        private string GetImage(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Books", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private string GetAudioVideo(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
    }
}