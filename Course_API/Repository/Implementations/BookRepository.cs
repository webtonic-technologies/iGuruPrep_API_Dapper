using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace Course_API.Repository.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string _connectionString;

        public BookRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> Add(BookDTO request)
        {

            try
            {
                var book = new Book
                {
                    BookName = request.BookName,
                    Status = true,
                    pathURL = ImageUpload(request.pathURL),
                    link = AudioVideoUpload(request.link),
                    createdon = DateTime.Now,
                    createdby = request.createdby,
                    EmployeeID = request.EmployeeID,
                    EmpFirstName = request.EmpFirstName,
                    FileTypeId = request.FileTypeId
                };
                string insertQuery = @"
        INSERT INTO [tblLibrary] 
            (BookName, Status, pathURL, link, createdon, createdby, EmployeeID, EmpFirstName, FileTypeId)
        VALUES 
            (@BookName, @Status, @pathURL, @link, @createdon, @createdby, @EmployeeID, @EmpFirstName, @FileTypeId);
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
                
                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo", book.pathURL);
                var filePath1 = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "BooksAudioVideo", book.link);
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
        public async Task<ServiceResponse<BookDTO>> Get(int id)
        {
            try
            {
                var response = new BookDTO();
                string selectQuery = @"
        SELECT 
            BookId,
            BookName,
            Status,
            pathURL,
            link,
            modifiedon,
            modifiedby,
            createdon,
            createdby,
            EmployeeID,
            EmpFirstName,
            FileTypeId
        FROM 
            [tblLibrary]
        WHERE 
            BookId = @BookId";
                var book = await _connection.QueryFirstOrDefaultAsync<Book>(selectQuery, new { BookId = id });
                if (book != null)
                {
                    response.BookId = book.BookId;
                    response.BookName = book.BookName;
                    response.Status = book.Status;
                    response.pathURL = GetImage(book.pathURL);
                    response.link = GetAudioVideo(book.link);
                    response.modifiedon = book.modifiedon;
                    response.modifiedby = book.modifiedby;
                    response.createdon = book.createdon;
                    response.createdby = book.createdby;
                    response.EmployeeID = book.EmployeeID;
                    response.EmpFirstName = book.EmpFirstName;
                    response.FileTypeId = book.FileTypeId;
                    response.BookAuthorDetails = GetListOfAuthorDettails(book.BookId);
                    response.BookSubjects = GetListOfBookSubject(book.BookId);
                    response.BookBoards = GetListOfBookBoards(book.BookId);
                    response.BookClasses = GetListOfBookClass(book.BookId);
                    response.BookCourses = GetListOfBookCourse(book.BookId);
                    response.BookExamTypes = GetListOfBookExamType(book.BookId);
                    response.BookCategories = GetListOfBookCategory(book.BookId);

                    return new ServiceResponse<BookDTO>(true, "Record Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<BookDTO>(false, "Record not Found", new BookDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<BookDTO>(false, ex.Message, new BookDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<BookDTO>>> GetAllBooks(BookListDTO request)
        {

            try
            {
                var bookIds = new HashSet<int>();

                // Define the queries
                string categoriesQuery = @"SELECT [BookId] FROM [tbllibraryCategory] WHERE [APId] = @APId";
                string boardsQuery = @"SELECT [bookID] FROM [tbllibraryBoard] WHERE [BoardID] = @BoardID";
                string classesQuery = @"SELECT [bookID] FROM [tbllibraryClass] WHERE [ClassID] = @ClassID";
                string coursesQuery = @"SELECT [bookID] FROM [tbllibraryCourse] WHERE [CourseID] = @CourseID";
                string examsQuery = @"SELECT [bookID] FROM [tbllibraryExamType] WHERE [ExamTypeID] = @ExamTypeID";
                string subjectQuery = @"SELECT [bookID] FROM [tbllibrarySubject] WHERE [SubjectID] = @SubjectID";

                var categoryTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(categoriesQuery, new { request.APId });
                });

                var boardTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(boardsQuery, new { request.BoardID });
                });

                var classTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(classesQuery, new { request.ClassID });
                });

                var courseTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(coursesQuery, new { request.CourseID });
                });

                var examTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(examsQuery, new { request.ExamTypeID });
                });

                var subjectTask = Task.Run(async () =>
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    return await connection.QueryAsync<int>(subjectQuery, new { request.SubjectID });
                });

                // Wait for all tasks to complete
                var results = await Task.WhenAll(categoryTask, boardTask, classTask, courseTask, examTask, subjectTask);

                // Add all results to the HashSet to ensure uniqueness
                foreach (var result in results)
                {
                    foreach (var id in result)
                    {
                        bookIds.Add(id);
                    }
                }

                // Prepare the list of IDs for the final query
                var parameters = new { Ids = bookIds.ToList() };

                // Main query to fetch magazine details


                string mainQuery = @"
        SELECT 
            BookId,
            BookName,
            Status,
            pathURL,
            link,
            modifiedon,
            modifiedby,
            createdon,
            createdby,
            EmployeeID,
            EmpFirstName,
            FileTypeId
        FROM 
            [tblLibrary]
        WHERE 
            BookId IN @Ids";

                var books = await _connection.QueryAsync<Book>(mainQuery, parameters);

                var response = books.Select(item => new BookDTO
                {
                    BookId = item.BookId,
                    BookName = item.BookName,
                    Status = item.Status,
                    pathURL = GetImage(item.pathURL),
                    link = GetAudioVideo(item.link),
                    modifiedon = item.modifiedon,
                    modifiedby = item.modifiedby,
                    createdon = item.createdon,
                    createdby = item.createdby,
                    EmployeeID = item.EmployeeID,
                    EmpFirstName = item.EmpFirstName,
                    FileTypeId = item.FileTypeId,
                    BookAuthorDetails = GetListOfAuthorDettails(item.BookId),
                    BookCategories = GetListOfBookCategory(item.BookId),
                    BookBoards = GetListOfBookBoards(item.BookId),
                    BookClasses = GetListOfBookClass(item.BookId),
                    BookCourses = GetListOfBookCourse(item.BookId),
                    BookExamTypes = GetListOfBookExamType(item.BookId),
                    BookSubjects = GetListOfBookSubject(item.BookId)
                }).ToList();

                return new ServiceResponse<List<BookDTO>>(true, "Records found", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<BookDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<string>> Update(BookDTO request)
        {
            try
            {
                var book = new Book
                {
                    BookName = request.BookName,
                    Status = true,
                    pathURL = ImageUpload(request.pathURL),
                    link = AudioVideoUpload(request.link),
                    modifiedon = DateTime.Now,
                    modifiedby = request.modifiedby,
                    EmployeeID = request.EmployeeID,
                    EmpFirstName = request.EmpFirstName,
                    FileTypeId = request.FileTypeId,
                    BookId = request.BookId
                };
                string updateQuery = @"
        UPDATE [tblLibrary]
        SET 
            BookName = @BookName,
            Status = @Status,
            pathURL = @pathURL,
            link = @link,
            modifiedon = @modifiedon,
            modifiedby = @modifiedby,
            EmployeeID = @EmployeeID,
            EmpFirstName = @EmpFirstName,
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
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
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
                    var insertquery = @"INSERT INTO [tbllibraryCategory] ([APId], [BookId], [APName])
                          VALUES (@APId, @BookId, @APName);";
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
                var insertquery = @"INSERT INTO [tbllibraryCategory] ([APId], [BookId], [APName])
                          VALUES (@APId, @BookId, @APName);";
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
                    var insertquery = @"INSERT INTO [tbllibraryClass] ([bookID], [ClassID], Name)
                          VALUES (@bookID, @ClassID, @Name);";
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
                var insertquery = @"INSERT INTO [tbllibraryClass] ([bookID], [ClassID], Name)
                          VALUES (@bookID, @ClassID, @Name);";
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
                    var insertquery = @"INSERT INTO [tbllibraryBoard] ([bookID], [BoardID], Name)
                          VALUES (@bookID, @BoardID, @Name);";
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
                var insertquery = @"INSERT INTO [tbllibraryBoard] ([bookID], [BoardID], Name)
                          VALUES (@bookID, @BoardID, @Name);";
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
                    var insertquery = @"INSERT INTO [tbllibraryCourse] ([bookID], [CourseID], Name)
                          VALUES (@bookID, @CourseID, @Name);";
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
                var insertquery = @"INSERT INTO [tbllibraryCourse] ([bookID], [CourseID], Name)
                          VALUES (@bookID, @CourseID, @Name);";
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
                    var insertquery = @"INSERT INTO [tbllibraryExamType] ([bookID], [ExamTypeID], Name)
                          VALUES (@bookID, @ExamTypeID, @Name);";
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
                var insertquery = @"INSERT INTO [tbllibraryExamType] ([bookID], [ExamTypeID], Name)
                          VALUES (@bookID, @ExamTypeID, @Name);";
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
                    var insertquery = @"INSERT INTO [tbllibrarySubject] ([bookID], [SubjectID], Name)
                          VALUES (@bookID, @SubjectID, @Name);";
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
                var insertquery = @"INSERT INTO [tbllibrarySubject] ([bookID], [SubjectID], Name)
                          VALUES (@bookID, @SubjectID, @Name);";
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
        private List<BookBoard> GetListOfBookBoards(int BookId)
        {
            var boardquery = @"SELECT * FROM [tbllibraryBoard] WHERE bookID = @bookID;";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookBoard>(boardquery, new { bookID = BookId });
            return data != null ? data.AsList() : [];
        }
        private List<BookCategory> GetListOfBookCategory(int BookId)
        {
            var query = @"SELECT * FROM [tbllibraryCategory] WHERE  BookId = @BookId;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookCategory>(query, new { BookId });
            return data != null ? data.AsList() : [];
        }
        private List<BookClass> GetListOfBookClass(int bookID)
        {
            var query = @"SELECT * FROM [tbllibraryClass] WHERE  bookID = @bookID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookClass>(query, new { bookID });
            return data != null ? data.AsList() : [];
        }
        private List<BookCourse> GetListOfBookCourse(int bookID)
        {
            var query = @"SELECT * FROM [tbllibraryCourse] WHERE  bookID = @bookID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookCourse>(query, new { bookID });
            return data != null ? data.AsList() : [];
        }
        private List<BookExamType> GetListOfBookExamType(int bookID)
        {
            var query = @"SELECT * FROM [tbllibraryExamType] WHERE  bookID = @bookID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookExamType>(query, new { bookID });
            return data != null ? data.AsList() : [];
        }
        private List<BookSubject> GetListOfBookSubject(int bookID)
        {
            var query = @"SELECT * FROM [tbllibrarySubject] WHERE  bookID = @bookID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookSubject>(query, new { bookID });
            return data != null ? data.AsList() : [];
        }
        private List<BookAuthorDetail> GetListOfAuthorDettails(int BookId)
        {
            var query = @"SELECT * FROM [tbllibraryAuthorDetails] WHERE  BookId = @BookId;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<BookAuthorDetail>(query, new { BookId });
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

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
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