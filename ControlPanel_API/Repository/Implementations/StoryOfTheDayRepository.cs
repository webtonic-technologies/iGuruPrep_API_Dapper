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
                INSERT INTO [iGuruPrep].[dbo].[tblSOTD] (
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
                UPDATE [iGuruPrep].[dbo].[tblSOTD]
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
                var response = new List<StoryOfTheDayDTO>();
                var query = "SELECT StoryId, EventTypeID,EventName,Event1PostDate,Event1Posttime," +
                    "Event2PostDate,Event2Posttime,Filename1,Filename2,Status,APName,eventtypename" +
                    ",modifiedon,modifiedby,createdon,createdby,EmployeeID,EmpFirstName FROM [dbo]." +
                    "[tblSOTD] WHERE 1 = 1";
                if (request.APID == 1)
                {
                    if (request.BoardId != 0)
                        query += " AND BoardId = @BoardId";
                    if (request.ClassId != 0)
                        query += " AND ClassId = @ClassId";
                    if (request.CourseId != 0)
                        query += " AND CourseId = @CourseId";
                    if (request.EventTypeId != 0)
                        query += " AND EventTypeId = @EventTypeId";

                    var storyOfTheDays = await _connection.QueryAsync<StoryOfTheDay>(query, request);
                    if (storyOfTheDays != null)
                    {
                        foreach (var data in storyOfTheDays)
                        {
                            var item = new StoryOfTheDayDTO
                            {
                                Filename1 = data.Filename1 != null ? GetStoryOfTheDayFileById(data.Filename1) : string.Empty,
                                Filename2 = data.Filename2 != null ? GetStoryOfTheDayFileById(data.Filename2) : string.Empty,
                                StoryId = data.StoryId,
                                EventTypeID = data.EventTypeID,
                                EventName = data.EventName,
                                Event1Posttime = data.Event1Posttime,
                                Event2Posttime = data.Event2Posttime,
                                Event1PostDate = data.Event1PostDate,
                                Event2PostDate = data.Event2PostDate,
                                modifiedby = data.modifiedby,
                                modifiedon = data.modifiedon,
                                createdon = data.createdon,
                                createdby = data.createdby,
                                eventtypename = data.eventtypename,
                                Status = data.Status,
                                EmployeeID = data.EmployeeID,
                                EmpFirstName = data.EmpFirstName,
                                SOTDBoards = GetListOfSOTDBoards(data.StoryId),
                                SOTDCategories = GetListOfSOTDCategory(data.StoryId),
                                SOTDClasses = GetListOfSOTDClass(data.StoryId),
                                SOTDCourses = GetListOfSOTDCourse(data.StoryId),
                                SOTDExamTypes = GetListOfSOTDExamType(data.StoryId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(false, "Records Not Found", [], 204);
                    }
                }
                else if (request.APID == 2)
                {
                    if (request.EventTypeId != 0)
                        query += " AND EventTypeId = @EventTypeId";
                    if (request.ExamType != 0)
                        query += " AND ExamType = @ExamType";

                    var storyOfTheDays = await _connection.QueryAsync<StoryOfTheDay>(query, request);
                    if (storyOfTheDays != null)
                    {
                        foreach (var data in storyOfTheDays)
                        {
                            var item = new StoryOfTheDayDTO
                            {
                                Filename1 = data.Filename1 != null ? GetStoryOfTheDayFileById(data.Filename1) : string.Empty,
                                Filename2 = data.Filename2 != null ? GetStoryOfTheDayFileById(data.Filename2) : string.Empty,
                                StoryId = data.StoryId,
                                EventTypeID = data.EventTypeID,
                                EventName = data.EventName,
                                Event1Posttime = data.Event1Posttime,
                                Event2Posttime = data.Event2Posttime,
                                Event1PostDate = data.Event1PostDate,
                                Event2PostDate = data.Event2PostDate,
                                modifiedby = data.modifiedby,
                                modifiedon = data.modifiedon,
                                createdon = data.createdon,
                                createdby = data.createdby,
                                eventtypename = data.eventtypename,
                                Status = data.Status,
                                EmployeeID = data.EmployeeID,
                                EmpFirstName = data.EmpFirstName,
                                SOTDBoards = GetListOfSOTDBoards(data.StoryId),
                                SOTDCategories = GetListOfSOTDCategory(data.StoryId),
                                SOTDClasses = GetListOfSOTDClass(data.StoryId),
                                SOTDCourses = GetListOfSOTDCourse(data.StoryId),
                                SOTDExamTypes = GetListOfSOTDExamType(data.StoryId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(false, "Records Not Found", [], 204);
                    }
                }
                else
                {
                    var getquery = @"
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
                FROM [iGuruPrep].[dbo].[tblSOTD];";
                    var storyOfTheDays = await _connection.QueryAsync<StoryOfTheDay>(getquery);
                    if (storyOfTheDays != null)
                    {
                        foreach (var data in storyOfTheDays)
                        {
                            var item = new StoryOfTheDayDTO
                            {
                                Filename1 = data.Filename1 != null ? GetStoryOfTheDayFileById(data.Filename1) : string.Empty,
                                Filename2 = data.Filename2 != null ? GetStoryOfTheDayFileById(data.Filename2) : string.Empty,
                                StoryId = data.StoryId,
                                EventTypeID = data.EventTypeID,
                                EventName = data.EventName,
                                Event1Posttime = data.Event1Posttime,
                                Event2Posttime = data.Event2Posttime,
                                Event1PostDate = data.Event1PostDate,
                                Event2PostDate = data.Event2PostDate,
                                modifiedby = data.modifiedby,
                                modifiedon = data.modifiedon,
                                createdon = data.createdon,
                                createdby = data.createdby,
                                eventtypename = data.eventtypename,
                                Status = data.Status,
                                EmployeeID = data.EmployeeID,
                                EmpFirstName = data.EmpFirstName,
                                SOTDBoards = GetListOfSOTDBoards(data.StoryId),
                                SOTDCategories = GetListOfSOTDCategory(data.StoryId),
                                SOTDClasses = GetListOfSOTDClass(data.StoryId),
                                SOTDCourses = GetListOfSOTDCourse(data.StoryId),
                                SOTDExamTypes = GetListOfSOTDExamType(data.StoryId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<StoryOfTheDayDTO>>(false, "Records Not Found", [], 204);
                    }
                }
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
                FROM [iGuruPrep].[dbo].[tblSOTD]
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
                var query = "SELECT EventTypeID, EventTypeName FROM [dbo].[tblSOTDEventtype];";
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
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = Guid.NewGuid().ToString() + ".jpg";
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
                throw new Exception("File not found");
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblSOTDCategory] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblSOTDCategory]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDCategory] ([APID], [SOTDID], [APIDName])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDCategory] ([APID], [SOTDID], [APIDName])
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblSOTDClass] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblSOTDClass]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDClass] ([SOTDID], [ClassID])
                          VALUES (@SOTDID, @ClassID);";
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDClass] ([SOTDID], [ClassID])
                          VALUES (@SOTDID, @ClassID);";
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblSOTDBoard] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblSOTDBoard]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDBoard] ([SOTDID], [BoardID])
                          VALUES (@SOTDID, @BoardID);";
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDBoard] ([SOTDID], [BoardID])
                          VALUES (@SOTDID, @BoardID);";
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblSOTDCourse] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblSOTDCourse]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDCourse] ([SOTDID], [CourseID])
                          VALUES (@SOTDID, @CourseID);";
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDCourse] ([SOTDID], [CourseID])
                          VALUES (@SOTDID, @CourseID);";
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblSOTDExamType] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblSOTDExamType]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDExamType] ([SOTDID], [ExamTypeID])
                          VALUES (@SOTDID, @ExamTypeID);";
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblSOTDExamType] ([SOTDID], [ExamTypeID])
                          VALUES (@SOTDID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<SOTDBoard> GetListOfSOTDBoards(int SOTDID)
        {
            var boardquery = @"
                SELECT 
                    tblSOTDBoardID,
                    SOTDID,
                    BoardID
                FROM 
                    [iGuruPrep].[dbo].[tblSOTDBoard]
                WHERE 
                    SOTDID = @SOTDID;";

            // Execute the SQL query with the SOTDID parameter
            var boardData = _connection.Query<SOTDBoard>(boardquery, new { SOTDID = SOTDID });
            return boardData != null ? boardData.AsList() : [];
        }
        private List<SOTDCategory> GetListOfSOTDCategory(int SOTDID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblSOTDCategory] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDCategory>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDClass> GetListOfSOTDClass(int SOTDID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblSOTDClass] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDClass>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDCourse> GetListOfSOTDCourse(int SOTDID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblSOTDCourse] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDCourse>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDExamType> GetListOfSOTDExamType(int SOTDID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblSOTDExamType] WHERE  SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDExamType>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
    }
}
