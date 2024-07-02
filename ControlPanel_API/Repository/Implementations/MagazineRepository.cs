using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class MagazineRepository : IMagazineRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string? _connectionString;

        public MagazineRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> AddUpdateMagazine(MagazineDTO request)
        {
            try
            {
                if (request.MagazineId == 0)
                {
                    var magazine = new Magazine
                    {
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        Link = PDFUpload(request.Link),
                        MagazineTitle = request.MagazineTitle,
                        PathURL = ImageUpload(request.PathURL),
                        Status = true,
                        EmployeeID = request.EmployeeID,
                        Time = request.Time,
                        Date = request.Date
                    };
                    string sql = @"INSERT INTO [tblMagazine] 
                   ([Date], [Time], [PathURL], [MagazineTitle], [Status], [Link], [EmployeeID], [createdon], [createdby]) 
                   VALUES 
                   (GETDATE(), @Time, @PathURL, @MagazineTitle, @Status, @Link, @EmployeeID, GETDATE(), @createdby);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    int insertedValue = await _connection.QueryFirstOrDefaultAsync<int>(sql, magazine);

                    if (insertedValue > 0)
                    {
                        int category = MagazineCategoryMapping(request.MagazineCategories ??= ([]), insertedValue);
                        int classes = MagazineClassMapping(request.MagazineClasses ??= ([]), insertedValue);
                        int board = MagazineBoardMapping(request.MagazineBoards ??= ([]), insertedValue);
                        int course = MagazineCourseMapping(request.MagazineCourses ??= ([]), insertedValue);
                        int exam = MagazineExamTypeMapping(request.MagazineExamTypes ??= ([]), insertedValue);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Magazine Added Successfully", 200);
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
                    string query = @"UPDATE [tblMagazine] 
                   SET [MagazineTitle] = @MagazineTitle, [PathURL] = @PathURL, [Link] = @Link, [EmployeeID] = @EmployeeID, [modifiedon] = GETDATE(), [modifiedby] = @modifiedby
                   WHERE [MagazineId] = @MagazineId";
                    var magazine = new Magazine
                    {
                        modifiedby = request.modifiedby,
                        modifiedon = DateTime.Now,
                        Link = PDFUpload(request.Link),
                        MagazineTitle = request.MagazineTitle,
                        PathURL = ImageUpload(request.PathURL),
                        Status = true,
                        EmployeeID = request.EmployeeID,
                        Time = request.Time,
                        MagazineId = request.MagazineId
                    };
                    int rowsAffected = await _connection.ExecuteAsync(query, magazine);
                    if (rowsAffected > 0)
                    {
                        int category = MagazineCategoryMapping(request.MagazineCategories ??= ([]), request.MagazineId);
                        int classes = MagazineClassMapping(request.MagazineClasses ??= ([]), request.MagazineId);
                        int board = MagazineBoardMapping(request.MagazineBoards ??= ([]), request.MagazineId);
                        int course = MagazineCourseMapping(request.MagazineCourses ??= ([]), request.MagazineId);
                        int exam = MagazineExamTypeMapping(request.MagazineExamTypes ??= ([]), request.MagazineId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Magazine updated Successfully", 200);
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
        public async Task<ServiceResponse<List<MagazineResponseDTO>>> GetAllMagazines(MagazineListDTO request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblMagazine]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                // Base query
                string baseQuery = @"
                SELECT DISTINCT
                    m.[MagazineId], 
                    m.[Date], 
                    m.[Time], 
                    m.[PathURL], 
                    m.[MagazineTitle], 
                    m.[Status], 
                    m.[Link], 
                    m.[EmployeeID],
                    m.[createdon], 
                    m.[createdby], 
                    m.[modifiedby], 
                    m.[modifiedon],
                    e.[EmpFirstName]
                FROM [tblMagazine] m
                LEFT JOIN [tblEmployee] e ON m.EmployeeID = e.Employeeid
                LEFT JOIN [tblMagazineCategory] mc ON m.MagazineId = mc.MagazineId
                LEFT JOIN [tblMagazineBoard] mb ON m.MagazineId = mb.MagazineId
                LEFT JOIN [tblMagazineClass] mcl ON m.MagazineId = mcl.MagazineId
                LEFT JOIN [tblMagazineCourse] mco ON m.MagazineId = mco.MagazineId
                LEFT JOIN [tblMagazineExamType] met ON m.MagazineId = met.MagazineId
                WHERE 1=1";

                // Applying filters
                if (request.ClassID > 0)
                {
                    baseQuery += " AND mcl.ClassID = @ClassID";
                }
                if (request.BoardIDID > 0)
                {
                    baseQuery += " AND mb.BoardIDID = @BoardIDID";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += " AND mco.CourseID = @CourseID";
                }
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND met.ExamTypeID = @ExamTypeID";
                }
                if (request.APID > 0)
                {
                    baseQuery += " AND mc.APID = @APID";
                }

                // Pagination
                baseQuery += " ORDER BY m.MagazineId OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var offset = (request.PageNumber - 1) * request.PageSize;

                // Parameters for the query
                var parameters = new
                {
                    ClassID = request.ClassID,
                    BoardIDID = request.BoardIDID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APID = request.APID,
                    Offset = offset,
                    PageSize = request.PageSize
                };

                // Fetch filtered and paginated records
                var mainResult = await _connection.QueryAsync<dynamic>(baseQuery, parameters);
                var today = DateTime.Today;
                // Map results to response DTO
                var response = mainResult.Where(item => item.Date <= today).Select(item => new MagazineResponseDTO
                {
                    MagazineId = item.MagazineId,
                    Date = item.Date,
                    Time = item.Time,
                    Link = GetPDF(item.Link),
                    PathURL = GetImage(item.PathURL),
                    MagazineTitle = item.MagazineTitle,
                    Status = item.Status,
                    modifiedon = item.modifiedon,
                    modifiedby = item.modifiedby,
                    createdon = item.createdon,
                    createdby = item.createdby,
                    EmployeeID = item.EmployeeID,
                    EmpFirstName = item.EmpFirstName,
                    MagazineCategories = GetListOfMagazineCategory(item.MagazineId),
                    MagazineBoards = GetListOfMagazineBoards(item.MagazineId),
                    MagazineClasses = GetListOfMagazineClass(item.MagazineId),
                    MagazineCourses = GetListOfMagazineCourse(item.MagazineId),
                    MagazineExamTypes = GetListOfMagazineExamType(item.MagazineId)
                }).ToList();

                // Check if there are records
                if (response.Count != 0)
                {
                    return new ServiceResponse<List<MagazineResponseDTO>>(true, "Records found", response, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<MagazineResponseDTO>>(false, "Records not found", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MagazineResponseDTO>>(false, ex.Message, new List<MagazineResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<MagazineResponseDTO>> GetMagazineById(int id)
        {
            try
            {
                var response = new MagazineResponseDTO();
                string query = @"
            SELECT m.[MagazineId], m.[Date], m.[Time], m.[PathURL], m.[MagazineTitle], m.[Status], m.[Link], m.[EmployeeID],
            m.[createdon], m.[createdby], m.[modifiedby], m.[modifiedon],
                   e.[EmpFirstName]
            FROM [tblMagazine] m
            LEFT JOIN [tblEmployee] e ON m.EmployeeID = e.Employeeid
            WHERE m.[MagazineId] = @MagazineId";

                var magazine = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new { MagazineId = id });
                var today = DateTime.Today;
                if (magazine != null)
                {
                    if (magazine.Date <= today)
                    {
                        response.Link = GetPDF(magazine.Link);
                        response.PathURL = GetImage(magazine.PathURL);
                        response.MagazineId = magazine.MagazineId;
                        response.Date = magazine.Date;
                        response.Time = magazine.Time;
                        response.MagazineTitle = magazine.MagazineTitle;
                        response.Status = magazine.Status;
                        response.EmpFirstName = magazine.EmpFirstName;
                        response.EmployeeID = magazine.EmployeeID;
                        response.createdby = magazine.createdby;
                        response.createdon = magazine.createdon;
                        response.modifiedon = magazine.modifiedon;
                        response.modifiedby = magazine.modifiedby;
                        response.MagazineCategories = GetListOfMagazineCategory(magazine.MagazineId);
                        response.MagazineBoards = GetListOfMagazineBoards(magazine.MagazineId);
                        response.MagazineClasses = GetListOfMagazineClass(magazine.MagazineId);
                        response.MagazineCourses = GetListOfMagazineCourse(magazine.MagazineId);
                        response.MagazineExamTypes = GetListOfMagazineExamType(magazine.MagazineId);

                        return new ServiceResponse<MagazineResponseDTO>(true, "Record Found", response, 200);
                    }
                    else
                    {
                        return new ServiceResponse<MagazineResponseDTO>(false, "Record not Found", new MagazineResponseDTO(), 500);
                    }
                }
                else
                {
                    return new ServiceResponse<MagazineResponseDTO>(false, "Record not Found", new MagazineResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MagazineResponseDTO>(false, ex.Message, new MagazineResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var magazine = await GetMagazineById(id);

                if (magazine.Data != null)
                {
                    magazine.Data.Status = !magazine.Data.Status;

                    string sql = "UPDATE [tblMagazine] SET Status = @Status WHERE [MagazineId] = @MagazineId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { magazine.Data.Status, MagazineId = id });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        public async Task<ServiceResponse<List<MagazineResponseDTO>>> GetMagazineByPublishDate(PublishMagazineDTO request)
        {
            try
            {
                var responseList = new List<MagazineResponseDTO>();
                string query = @"
            SELECT m.[MagazineId], m.[Date], m.[Time], m.[PathURL], m.[MagazineTitle], m.[Status], m.[Link], m.[EmployeeID],
            m.[createdon], m.[createdby], m.[modifiedby], m.[modifiedon],
            e.[EmpFirstName]
            FROM [tblMagazine] m
            LEFT JOIN [tblEmployee] e ON m.EmployeeID = e.Employeeid
            WHERE m.[Date] = @Date AND m.[Time] = @Time";

                var magazines = await _connection.QueryAsync<dynamic>(query, new { Date = request.Date, Time = request.Time });

                foreach (var magazine in magazines)
                {
                    var response = new MagazineResponseDTO
                    {
                        Link = GetPDF(magazine.Link),
                        PathURL = GetImage(magazine.PathURL),
                        MagazineId = magazine.MagazineId,
                        Date = magazine.Date,
                        Time = magazine.Time,
                        MagazineTitle = magazine.MagazineTitle,
                        Status = magazine.Status,
                        EmpFirstName = magazine.EmpFirstName,
                        EmployeeID = magazine.EmployeeID,
                        createdby = magazine.createdby,
                        createdon = magazine.createdon,
                        modifiedon = magazine.modifiedon,
                        modifiedby = magazine.modifiedby,
                        MagazineCategories = GetListOfMagazineCategory(magazine.MagazineId),
                        MagazineBoards = GetListOfMagazineBoards(magazine.MagazineId),
                        MagazineClasses = GetListOfMagazineClass(magazine.MagazineId),
                        MagazineCourses = GetListOfMagazineCourse(magazine.MagazineId),
                        MagazineExamTypes = GetListOfMagazineExamType(magazine.MagazineId)
                    };

                    responseList.Add(response);
                }

                return new ServiceResponse<List<MagazineResponseDTO>>(true, "Operation Successful", responseList, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MagazineResponseDTO>>(false, ex.Message, new List<MagazineResponseDTO>(), 500);
            }
        }
        private string ImageUpload(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine");

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
        private string PDFUpload(string pdf)
        {
            if (string.IsNullOrEmpty(pdf) || pdf == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(pdf);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "MagazinePDF");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsPdf(imageData) == true ? ".pdf" : string.Empty;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string GetImage(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "MagazinePDF", Filename);
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private int MagazineCategoryMapping(List<MagazineCategory> request, int MagazineId)
        {
            foreach (var data in request)
            {
                data.MagazineId = MagazineId;
            }
            string query = "SELECT COUNT(*) FROM [tblMagazineCategory] WHERE [MagazineId] = @MagazineId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblMagazineCategory]
                          WHERE [MagazineId] = @MagazineId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblMagazineCategory] ([APID], [MagazineId])
                          VALUES (@APID, @MagazineId);";
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
                var insertquery = @"INSERT INTO [tblMagazineCategory] ([APID], [MagazineId])
                          VALUES (@APID, @MagazineId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineClassMapping(List<MagazineClass> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [tblMagazineClass] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblMagazineClass]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblMagazineClass] ([MagazineID], [ClassID])
                          VALUES (@MagazineID, @ClassID);";
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
                var insertquery = @"INSERT INTO [tblMagazineClass] ([MagazineID], [ClassID])
                          VALUES (@MagazineID, @ClassID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineBoardMapping(List<MagazineBoard> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [tblMagazineBoard] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblMagazineBoard]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblMagazineBoard] ([MagazineID], [BoardIDID])
                          VALUES (@MagazineID, @BoardIDID);";
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
                var insertquery = @"INSERT INTO [tblMagazineBoard] ([MagazineID], [BoardIDID])
                          VALUES (@MagazineID, @BoardIDID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineCourseMapping(List<MagazineCourse> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [tblMagazineCourse] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblMagazineCourse]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblMagazineCourse] ([MagazineID], [CourseID])
                          VALUES (@MagazineID, @CourseID);";
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
                var insertquery = @"INSERT INTO [tblMagazineCourse] ([MagazineID], [CourseID])
                          VALUES (@MagazineID, @CourseID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineExamTypeMapping(List<MagazineExamType> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [tblMagazineExamType] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblMagazineExamType]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblMagazineExamType] ([MagazineID], [ExamTypeID])
                          VALUES (@MagazineID, @ExamTypeID);";
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
                var insertquery = @"INSERT INTO [tblMagazineExamType] ([MagazineID], [ExamTypeID])
                          VALUES (@MagazineID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<MagazineBoardResponse> GetListOfMagazineBoards(int MagazineID)
        {
            string boardQuery = @"
            SELECT mb.[MagazineBoardId], mb.[MagazineID], mb.[BoardIDID], b.[BoardName] AS Name
            FROM [tblMagazineBoard] mb
            LEFT JOIN [tblBoard] b ON mb.BoardIDID = b.BoardID
            WHERE mb.[MagazineID] = @MagazineID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineBoardResponse>(boardQuery, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineCategoryResponse> GetListOfMagazineCategory(int MagazineId)
        {
            string categoryQuery = @"
            SELECT mc.[MgCategoryID], mc.[MagazineId], mc.[APID], c.[APName] AS APIDName
            FROM [tblMagazineCategory] mc
            LEFT JOIN [tblCategory] c ON mc.APID = c.APID
            WHERE mc.[MagazineId] = @MagazineId";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineCategoryResponse>(categoryQuery, new { MagazineId });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineClassResponse> GetListOfMagazineClass(int MagazineID)
        {
            string classQuery = @"
            SELECT mcl.[MagazineClassId], mcl.[MagazineID], mcl.[ClassID], cl.[ClassName] AS Name
            FROM [tblMagazineClass] mcl
            LEFT JOIN [tblClass] cl ON mcl.ClassID = cl.ClassID
            WHERE mcl.[MagazineID] = @MagazineID";
            var data = _connection.Query<MagazineClassResponse>(classQuery, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineCourseResponse> GetListOfMagazineCourse(int MagazineID)
        {
            string courseQuery = @"
            SELECT mcr.[MagazineCourseID], mcr.[MagazineID], mcr.[CourseID], cr.[CourseName] AS Name
            FROM [tblMagazineCourse] mcr
            LEFT JOIN [tblCourse] cr ON mcr.CourseID = cr.CourseID
            WHERE mcr.[MagazineID] = @MagazineID";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineCourseResponse>(courseQuery, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineExamTypeResponse> GetListOfMagazineExamType(int MagazineID)
        {
            string examTypeQuery = @"
            SELECT met.[MagazineExamTypeID], met.[MagazineID], met.[ExamTypeID], et.[ExamTypeName] AS Name
            FROM [tblMagazineExamType] met
            LEFT JOIN [tblExamType] et ON met.ExamTypeID = et.ExamTypeID
            WHERE met.[MagazineID] = @MagazineID";

            var data = _connection.Query<MagazineExamTypeResponse>(examTypeQuery, new { MagazineID });
            return data != null ? data.AsList() : [];
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
        private bool IsPdf(byte[] fileData)
        {
            return fileData.Length > 4 &&
                   fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46;
        }
    }
}
