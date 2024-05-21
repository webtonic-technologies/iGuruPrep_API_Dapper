using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class StoryOfTheDayRepository : IStoryOfTheDayRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StoryOfTheDayRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddNewStoryOfTheDay(StoryOfTheDayDTO request)
        {
            try
            {
                var storyOfTheDay = new StoryOfTheDay
                {
                    createdby = request.createdby,
                    createdon = DateTime.Now,
                    Event1Posttime = request.Event1Posttime,
                    Event2Posttime = request.Event2Posttime,
                    EmpFirstName = request.EmpFirstName,
                    EmployeeID = request.EmployeeID,
                    Event1PostDate = request.Event1PostDate,
                    Event2PostDate = request.Event2PostDate,
                    EventName = request.EventName,
                    EventTypeID = request.EventTypeID,
                    eventtypename = request.eventtypename,
                    Status = true,
                    Filename1 = request.Filename1 != null ? ImageUpload(request.Filename1) : string.Empty,
                    Filename2 = request.Filename2 != null ? ImageUpload(request.Filename2) : string.Empty,
                };
                var query = @"
                INSERT INTO [tblSOTD] (
                    createdby, createdon, Event1Posttime, Event2Posttime, EmpFirstName, 
                    EmployeeID, Event1PostDate, Event2PostDate, EventName, EventTypeID, 
                    eventtypename, Status, Filename1, Filename2)
                VALUES (
                    @createdby, @createdon, @Event1Posttime, @Event2Posttime, @EmpFirstName, 
                    @EmployeeID, @Event1PostDate, @Event2PostDate, @EventName, @EventTypeID, 
                    @eventtypename, @Status, @Filename1, @Filename2);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                // Execute the SQL insert command with parameters
                int insertedValue = await _connection.QueryFirstOrDefaultAsync<int>(query, storyOfTheDay);
                if (insertedValue > 0)
                {
                    int category = SOTDCategoryMapping(request.SOTDCategories ??= ([]), insertedValue);
                    int classes = SOTDClassMapping(request.SOTDClasses ??= ([]), insertedValue);
                    int board = SOTDBoardMapping(request.SOTDBoards ??= ([]), insertedValue);
                    int course = SOTDCourseMapping(request.SOTDCourses ??= ([]), insertedValue);
                    int exam = SOTDExamTypeMapping(request.SOTDExamTypes ??= ([]), insertedValue);
                    if(category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "SOTD Added Successfully", 200);
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
        public async Task<ServiceResponse<string>> UpdateStoryOfTheDay(StoryOfTheDayDTO request)
        {
            try
            {
                var query = @"
                UPDATE [tblSOTD]
                SET 
                    modifiedby = @modifiedby,
                    modifiedon = @modifiedon,
                    Event1Posttime = @Event1Posttime,
                    Event2Posttime = @Event2Posttime,
                    EmpFirstName = @EmpFirstName,
                    EmployeeID = @EmployeeID,
                    Event1PostDate = @Event1PostDate,
                    Event2PostDate = @Event2PostDate,
                    EventName = @EventName,
                    EventTypeID = @EventTypeID,
                    eventtypename = @eventtypename,
                    Status = @Status,
                    Filename1 = @Filename1,
                    Filename2 = @Filename2
                WHERE StoryId = @StoryId;"; // Add condition for the specific record to update

                var storyOfTheDay = new StoryOfTheDay
                {
                    modifiedby = request.modifiedby,
                    modifiedon = DateTime.Now,
                    Event1Posttime = request.Event1Posttime,
                    Event2Posttime = request.Event2Posttime,
                    EmpFirstName = request.EmpFirstName,
                    EmployeeID = request.EmployeeID,
                    Event1PostDate = request.Event1PostDate,
                    Event2PostDate = request.Event2PostDate,
                    EventName = request.EventName,
                    EventTypeID = request.EventTypeID,
                    eventtypename = request.eventtypename,
                    Status = request.Status,
                    Filename1 = request.Filename1 != null ? ImageUpload(request.Filename1) : string.Empty,
                    Filename2 = request.Filename2 != null ? ImageUpload(request.Filename2) : string.Empty,
                    StoryId = request.StoryId
                };
                // Execute the SQL update command with parameters
                int rowsAffected = await _connection.ExecuteAsync(query, storyOfTheDay);

                if (rowsAffected > 0)
                {
                    int category = SOTDCategoryMapping(request.SOTDCategories ??= ([]), request.StoryId);
                    int classes = SOTDClassMapping(request.SOTDClasses ??= ([]), request.StoryId);
                    int board = SOTDBoardMapping(request.SOTDBoards ??= ([]), request.StoryId);
                    int course = SOTDCourseMapping(request.SOTDCourses ??= ([]), request.StoryId);
                    int exam = SOTDExamTypeMapping(request.SOTDExamTypes ??= ([]), request.StoryId);
                    if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "SOTD updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "Record not found", 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(
                    "SELECT Filename1, Filename2 FROM tblSOTD WHERE StoryId = @StoryId",
                    new { StoryId = id });

                if (storyOfTheDay == null)
                {
                    throw new Exception("Story of the day not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.Filename1);
                var filePath1 = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.Filename2);
                if (File.Exists(filePath) || File.Exists(filePath1))
                {
                    File.Delete(filePath);
                    File.Delete(filePath1);
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "DELETE FROM tblSOTD WHERE StoryId = @StoryId",
                    new { StoryId = id });

                if (rowsAffected > 0)
                {
                    var deleteCat = @"DELETE FROM [tblSOTDCategory]
                          WHERE [SOTDID] = @SOTDID;";
                    var delCat = _connection.Execute(deleteCat, new { SOTDID = id });

                    var deleteClass = @"DELETE FROM [tblSOTDClass]
                          WHERE [SOTDID] = @SOTDID;";
                    var delClass = _connection.Execute(deleteClass, new { SOTDID = id });

                    var deleteBoard = @"DELETE FROM [tblSOTDBoard]
                          WHERE [SOTDID] = @SOTDID;";
                    var delBoard = _connection.Execute(deleteBoard, new { SOTDID = id });

                    var deleteCourse = @"DELETE FROM [tblSOTDCourse]
                          WHERE [SOTDID] = @SOTDID;";
                    var delCourse = _connection.Execute(deleteCourse, new { SOTDID = id });

                    var deleteExamType = @"DELETE FROM [tblSOTDExamType]
                          WHERE [SOTDID] = @SOTDID;";
                    var delExamType = _connection.Execute(deleteExamType, new { SOTDID = id });

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
        public async Task<ServiceResponse<List<StoryOfTheDayDTO>>> GetAllStoryOfTheDay(SOTDListDTO request)
        {
            try
            {
                var SOTDIds = new HashSet<int>();

                // Define the queries
                string categoriesQuery = @"SELECT [SOTDID] FROM [tblSOTDCategory] WHERE [APID] = @APID";
                string boardsQuery = @"SELECT [SOTDID] FROM [tblSOTDBoard] WHERE [BoardID] = @BoardID";
                string classesQuery = @"SELECT SOTDID FROM [tblSOTDClass] WHERE [ClassID] = @ClassID";
                string coursesQuery = @"SELECT SOTDID FROM [tblSOTDCourse] WHERE [CourseID] = @CourseID";
                string examsQuery = @"SELECT SOTDID FROM [tblSOTDExamType] WHERE [ExamTypeID] = @ExamTypeID";
                string sotdQuery = @"SELECT SOTDID FROM [tblSOTD] WHERE [EventTypeID] = @EventTypeID";

                // Create tasks for concurrent execution
                var categoryTask = _connection.QueryAsync<int>(categoriesQuery, new { request.APID });
                var boardTask = _connection.QueryAsync<int>(boardsQuery, new { request.BoardID });
                var classTask = _connection.QueryAsync<int>(classesQuery, new { request.ClassID });
                var courseTask = _connection.QueryAsync<int>(coursesQuery, new { request.CourseID });
                var examTask = _connection.QueryAsync<int>(examsQuery, new { request.ExamTypeID });
                var sotdTask = _connection.QueryAsync<int>(sotdQuery, new { request.EventTypeID });

                // Wait for all tasks to complete
                var results = await Task.WhenAll(categoryTask, boardTask, classTask, courseTask, examTask, sotdTask);

                // Add all results to the HashSet to ensure uniqueness
                foreach (var result in results)
                {
                    foreach (var id in result)
                    {
                        SOTDIds.Add(id);
                    }
                }

                // Prepare the list of IDs for the final query
                var parameters = new { Ids = SOTDIds.ToList() };

                // Main query to fetch magazine details

                string mainQuery = @"
                SELECT 
                    [StoryId],
                    [EventTypeID],
                    [EventName],
                    [Event1Posttime],
                    [Event1PostDate],
                    [Event2PostDate],
                    [Event2Posttime],
                    [modifiedby],
                    [createdby],
                    [eventtypename],
                    [modifiedon],
                    [createdon],
                    [Status],
                    [EmployeeID],
                    [Filename1],
                    [Filename2],
                    [EmpFirstName]
                FROM [tblSOTD]
                WHERE [StoryId] IN @Ids";

                var SOTDs = await _connection.QueryAsync<StoryOfTheDay>(mainQuery, parameters);

                var response = SOTDs.Select(item => new StoryOfTheDayDTO
                {
                    StoryId = item.StoryId,
                    EventTypeID = item.EventTypeID,
                    EventName = item.EventName,
                    Event1Posttime = item.Event1Posttime,
                    Event1PostDate = item.Event1PostDate,
                    Event2PostDate = item.Event2PostDate,
                    Event2Posttime = item.Event2Posttime,
                    modifiedby = item.modifiedby,
                    createdby = item.createdby,
                    eventtypename = item.eventtypename,
                    modifiedon = item.modifiedon,
                    createdon = item.createdon,
                    Status = item.Status,
                    EmployeeID = item.EmployeeID,
                    Filename1 = GetStoryOfTheDayFileById(item.Filename1),
                    Filename2 = GetStoryOfTheDayFileById(item.Filename2),
                    EmpFirstName = item.EmpFirstName,
                    SOTDCategories = GetListOfSOTDCategory(item.StoryId),
                    SOTDBoards = GetListOfSOTDBoards(item.StoryId),
                    SOTDClasses = GetListOfSOTDClass(item.StoryId),
                    SOTDCourses = GetListOfSOTDCourse(item.StoryId),
                    SOTDExamTypes = GetListOfSOTDExamType(item.StoryId)
                }).ToList();

                return new ServiceResponse<List<StoryOfTheDayDTO>>(true, "Records found", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StoryOfTheDayDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                var response = new StoryOfTheDayDTO();
                var query = @"
                SELECT 
                    StoryId,
                    EventTypeID,
                    EventName,
                    Event1PostDate,
                    Event1Posttime,
                    Event2PostDate,
                    Event2Posttime,
                    Filename1,
                    Filename2,
                    Status,
                    APName,
                    eventtypename,
                    modifiedon,
                    modifiedby,
                    createdon,
                    createdby,
                    EmployeeID,
                    EmpFirstName
                FROM [tblSOTD]
                WHERE StoryId = @StoryId;";
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(query, new { StoryId = id });

                if (storyOfTheDay != null)
                {
                    response.Filename1 = storyOfTheDay.Filename1 != null ? GetStoryOfTheDayFileById(storyOfTheDay.Filename1) : string.Empty;
                    response.Filename2 = storyOfTheDay.Filename2 != null ? GetStoryOfTheDayFileById(storyOfTheDay.Filename2) : string.Empty;
                    response.StoryId = storyOfTheDay.StoryId;
                    response.EventTypeID = storyOfTheDay.EventTypeID;
                    response.EventName = storyOfTheDay.EventName;
                    response.Event1Posttime = storyOfTheDay.Event1Posttime;
                    response.Event2Posttime = storyOfTheDay.Event2Posttime;
                    response.Event1PostDate = storyOfTheDay.Event1PostDate;
                    response.Event2PostDate = storyOfTheDay.Event2PostDate;
                    response.modifiedby = storyOfTheDay.modifiedby;
                    response.modifiedon = storyOfTheDay.modifiedon;
                    response.createdon = storyOfTheDay.createdon;
                    response.createdby = storyOfTheDay.createdby;
                    response.eventtypename = storyOfTheDay.eventtypename;
                    response.Status = storyOfTheDay.Status;
                    response.EmployeeID = storyOfTheDay.EmployeeID;
                    response.EmpFirstName = storyOfTheDay.EmpFirstName;
                    response.SOTDBoards = GetListOfSOTDBoards(id);
                    response.SOTDCategories = GetListOfSOTDCategory(id);
                    response.SOTDClasses = GetListOfSOTDClass(id);
                    response.SOTDCourses = GetListOfSOTDCourse(id);
                    response.SOTDExamTypes = GetListOfSOTDExamType(id);

                    return new ServiceResponse<StoryOfTheDayDTO>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<StoryOfTheDayDTO>(false, "Opertion Failed", new StoryOfTheDayDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StoryOfTheDayDTO>(false, ex.Message, new StoryOfTheDayDTO(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var sotd = await GetStoryOfTheDayById(id);

                if (sotd.Data != null)
                {
                    sotd.Data.Status = !sotd.Data.Status;

                    string sql = "UPDATE [tblSOTD] SET Status = @Status WHERE [StoryId] = @StoryId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { sotd.Data.Status, StoryId = id });
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
        public async Task<ServiceResponse<List<EventType>>> GetEventtypeList()
        {
            try
            {
                var query = "SELECT EventTypeID, EventTypeName FROM [tblSOTDEventtype];";
                var eventTypes = await _connection.QueryAsync<EventType>(query);
                if (eventTypes != null)
                {
                    return new ServiceResponse<List<EventType>>(true, "Operation Successful", eventTypes.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<EventType>>(false, "Opertion Failed", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<EventType>>(false, ex.Message, [], 500);
            }
        }
        private string ImageUpload(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay");

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
        private string GetStoryOfTheDayFileById(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private int SOTDCategoryMapping(List<SOTDCategory> request, int SOTDID)
        {
            foreach (var data in request)
            {
                data.SOTDID = SOTDID;
            }
            string query = "SELECT COUNT(*) FROM [tblSOTDCategory] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDCategory]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDCategory] ([APID], [SOTDID], [APIDName])
                          VALUES (@APID, @SOTDID, @APIDName);";
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
                var insertquery = @"INSERT INTO [tblSOTDCategory] ([APID], [SOTDID], [APIDName])
                          VALUES (@APID, @SOTDID, @APIDName);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int SOTDClassMapping(List<SOTDClass> request, int SOTDID)
        {
            foreach (var data in request)
            {
                data.SOTDID = SOTDID;
            }
            string query = "SELECT COUNT(*) FROM [tblSOTDClass] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDClass]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDClass] ([SOTDID], [ClassID], Name)
                          VALUES (@SOTDID, @ClassID, @Name);";
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
                var insertquery = @"INSERT INTO [tblSOTDClass] ([SOTDID], [ClassID], Name)
                          VALUES (@SOTDID, @ClassID, @Name);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int SOTDBoardMapping(List<SOTDBoard> request, int SOTDID)
        {
            foreach (var data in request)
            {
                data.SOTDID = SOTDID;
            }
            string query = "SELECT COUNT(*) FROM [tblSOTDBoard] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDBoard]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDBoard] ([SOTDID], [BoardID], Name)
                          VALUES (@SOTDID, @BoardID, @Name);";
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
                var insertquery = @"INSERT INTO [tblSOTDBoard] ([SOTDID], [BoardID], Name)
                          VALUES (@SOTDID, @BoardID, @Name);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int SOTDCourseMapping(List<SOTDCourse> request, int SOTDID)
        {
            foreach (var data in request)
            {
                data.SOTDID = SOTDID;
            }
            string query = "SELECT COUNT(*) FROM [tblSOTDCourse] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDCourse]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDCourse] ([SOTDID], [CourseID], Name)
                          VALUES (@SOTDID, @CourseID, @Name);";
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
                var insertquery = @"INSERT INTO [tblSOTDCourse] ([SOTDID], [CourseID], Name)
                          VALUES (@SOTDID, @CourseID, @Name);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int SOTDExamTypeMapping(List<SOTDExamType> request, int SOTDID)
        {
            foreach (var data in request)
            {
                data.SOTDID = SOTDID;
            }
            string query = "SELECT COUNT(*) FROM [tblSOTDExamType] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDExamType]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDExamType] ([SOTDID], [ExamTypeID], Name)
                          VALUES (@SOTDID, @ExamTypeID, @Name);";
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
                var insertquery = @"INSERT INTO [tblSOTDExamType] ([SOTDID], [ExamTypeID], Name)
                          VALUES (@SOTDID, @ExamTypeID, @Name);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<SOTDBoard> GetListOfSOTDBoards(int SOTDID)
        {
            var boardquery = @"
                SELECT *
                FROM 
                    [tblSOTDBoard]
                WHERE 
                    SOTDID = @SOTDID;";

            // Execute the SQL query with the SOTDID parameter
            var boardData = _connection.Query<SOTDBoard>(boardquery, new { SOTDID = SOTDID });
            return boardData != null ? boardData.AsList() : [];
        }
        private List<SOTDCategory> GetListOfSOTDCategory(int SOTDID)
        {
            var query = @"SELECT * FROM [tblSOTDCategory] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDCategory>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDClass> GetListOfSOTDClass(int SOTDID)
        {
            var query = @"SELECT * FROM [tblSOTDClass] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDClass>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDCourse> GetListOfSOTDCourse(int SOTDID)
        {
            var query = @"SELECT * FROM [tblSOTDCourse] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDCourse>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDExamType> GetListOfSOTDExamType(int SOTDID)
        {
            var query = @"SELECT * FROM [tblSOTDExamType] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDExamType>(query, new { SOTDID });
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
    }
}
