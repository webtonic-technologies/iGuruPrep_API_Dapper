﻿using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace ControlPanel_API.Repository.Implementations
{
    public class StoryOfTheDayRepository : IStoryOfTheDayRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string? _connectionString;
        public StoryOfTheDayRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> AddUpdateStoryOfTheDay(StoryOfTheDayDTO request)
        {
            try
            {
                if (request.StoryId == 0)
                {
                    var storyOfTheDay = new StoryOfTheDay
                    {
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        Event1Posttime = request.Event1Posttime,
                        Event2Posttime = request.Event2Posttime,
                        EmployeeID = request.EmployeeID,
                        Event1PostDate = request.Event1PostDate,
                        Event2PostDate = request.Event2PostDate,
                        EventTypeID = request.EventTypeID,
                        eventname = request.EventName,
                        Status = true,
                        Event1Image = request.Event1Image != null ? ImageUpload(request.Event1Image) : string.Empty,
                        Event2Image = request.Event2Image != null ? ImageUpload(request.Event2Image) : string.Empty,
                    };
                    var query = @"
                INSERT INTO [tblSOTD] (
                    createdby, createdon, Event1Posttime, Event2Posttime, 
                    EmployeeID, Event1PostDate, Event2PostDate, EventTypeID, 
                    eventname, Status, Event1Image, Event2Image)
                VALUES (
                    @createdby, @createdon, @Event1Posttime, @Event2Posttime, 
                    @EmployeeID, @Event1PostDate, @Event2PostDate, @EventTypeID, 
                    @eventname, @Status, @Event1Image, @Event2Image);
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
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
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
                else
                {
                    var query = @"
                UPDATE [tblSOTD]
                SET 
                    modifiedby = @modifiedby,
                    modifiedon = @modifiedon,
                    Event1Posttime = @Event1Posttime,
                    Event2Posttime = @Event2Posttime,
                    EmployeeID = @EmployeeID,
                    Event1PostDate = @Event1PostDate,
                    Event2PostDate = @Event2PostDate,
                    EventTypeID = @EventTypeID,
                    eventname = @eventname,
                    Status = @Status,
                    Event1Image = @Event1Image,
                    Event2Image = @Event2Image
                WHERE StoryId = @StoryId;"; // Add condition for the specific record to update

                    var storyOfTheDay = new StoryOfTheDay
                    {
                        modifiedby = request.modifiedby,
                        modifiedon = DateTime.Now,
                        Event1Posttime = request.Event1Posttime,
                        Event2Posttime = request.Event2Posttime,
                        EmployeeID = request.EmployeeID,
                        Event1PostDate = request.Event1PostDate,
                        Event2PostDate = request.Event2PostDate,
                        EventTypeID = request.EventTypeID,
                        eventname = request.EventName,
                        Status = request.Status,
                        Event1Image = request.Event1Image != null ? ImageUpload(request.Event1Image) : string.Empty,
                        Event2Image = request.Event2Image != null ? ImageUpload(request.Event2Image) : string.Empty,
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

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.Event1Image);
                var filePath1 = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.Event2Image);
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
        public async Task<ServiceResponse<List<StoryOfTheDayResponseDTO>>> GetAllStoryOfTheDay(SOTDListDTO request)
        {
            try
            {
                var employeeRoleQuery = "SELECT e.RoleID, r.RoleCode FROM tblEmployee e INNER JOIN tblRole r ON e.RoleID = r.RoleID WHERE e.Employeeid = @EmployeeID";
                var employeeRole = await _connection.QuerySingleOrDefaultAsync<dynamic>(employeeRoleQuery, new { EmployeeID = request.EmployeeId });

                // Determine if the employee is Admin or SuperAdmin
                bool isAdminOrSuperAdmin = employeeRole != null && (employeeRole.RoleCode == "AD" || employeeRole.RoleCode == "SA");
                string baseQuery;
                // Base query to fetch all matching records
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery = @"SELECT DISTINCT
                s.StoryId,
                s.EventTypeID,
                s.Event1PostDate,
                s.Event1Posttime,
                s.Event2PostDate,
                s.Event2Posttime,
                s.Event1Image,
                s.Event2Image,
                s.Status,
                s.APName,
                s.eventname,
                s.modifiedon,
                s.modifiedby,
                s.createdon,
                s.createdby,
                s.EmployeeID,
                ev.EventTypeName AS eventtypename,
                e.EmpFirstName,
                CASE 
                    WHEN CAST(s.Event1PostDate AS DATETIME) + CAST(s.Event1Posttime AS DATETIME) <= GETDATE()
                         AND DATEADD(HOUR, -24, GETDATE()) <= CAST(s.Event1PostDate AS DATETIME) + CAST(s.Event1Posttime AS DATETIME)
                    THEN 1 
                    ELSE 0 
                END AS IsEvent1Valid,
                CASE 
                    WHEN CAST(s.Event2PostDate AS DATETIME) + CAST(s.Event2Posttime AS DATETIME) <= GETDATE()
                         AND DATEADD(HOUR, -24, GETDATE()) <= CAST(s.Event2PostDate AS DATETIME) + CAST(s.Event2Posttime AS DATETIME)
                    THEN 1 
                    ELSE 0 
                END AS IsEvent2Valid
            FROM [tblSOTD] s
            LEFT JOIN [tblEmployee] e ON s.EmployeeID = e.Employeeid
            LEFT JOIN tblSOTDEventtype ev ON s.EventTypeID = ev.EventTypeID
            LEFT JOIN [tblSOTDCategory] sc ON s.StoryId = sc.SOTDID
            LEFT JOIN [tblSOTDBoard] sb ON s.StoryId = sb.SOTDID
            LEFT JOIN [tblSOTDClass] scl ON s.StoryId = scl.SOTDID
            LEFT JOIN [tblSOTDCourse] sco ON s.StoryId = sco.SOTDID
            LEFT JOIN [tblSOTDExamType] se ON s.StoryId = se.SOTDID
            WHERE 1=1;
            ";
                }
                else
                {
                    baseQuery = @"SELECT DISTINCT
                s.StoryId,
                s.EventTypeID,
                s.Event1PostDate,
                s.Event1Posttime,
                s.Event2PostDate,
                s.Event2Posttime,
                s.Event1Image,
                s.Event2Image,
                s.Status,
                s.APName,
                s.eventname,
                s.modifiedon,
                s.modifiedby,
                s.createdon,
                s.createdby,
                s.EmployeeID,
                ev.EventTypeName AS eventtypename,
                e.EmpFirstName,
            FROM [tblSOTD] s
            LEFT JOIN [tblEmployee] e ON s.EmployeeID = e.Employeeid
            LEFT JOIN tblSOTDEventtype ev ON s.EventTypeID = ev.EventTypeID
            LEFT JOIN [tblSOTDCategory] sc ON s.StoryId = sc.SOTDID
            LEFT JOIN [tblSOTDBoard] sb ON s.StoryId = sb.SOTDID
            LEFT JOIN [tblSOTDClass] scl ON s.StoryId = scl.SOTDID
            LEFT JOIN [tblSOTDCourse] sco ON s.StoryId = sco.SOTDID
            LEFT JOIN [tblSOTDExamType] se ON s.StoryId = se.SOTDID
            WHERE 1=1;
            ";
                }

                // Applying filters
                if (request.BoardID > 0)
                {
                    baseQuery += @"
    AND s.StoryId IN (
        SELECT sb.[SOTDID] 
        FROM [tblSOTDBoard] sb 
        INNER JOIN [tblBoard] b ON sb.[BoardID] = b.[BoardId] 
        WHERE b.[Status] = 1 AND sb.[BoardID] = @BoardID
    )";
                }
                if (request.ClassID > 0)
                {
                    baseQuery += @"
    AND s.StoryId IN (
        SELECT scl.[SOTDID] 
        FROM [tblSOTDClass] scl 
        INNER JOIN [tblClass] c ON scl.[ClassID] = c.[ClassId] 
        WHERE c.[Status] = 1 AND scl.[ClassID] = @ClassID
    )";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += @"
    AND s.StoryId IN (
        SELECT sco.[SOTDID] 
        FROM [tblSOTDCourse] sco 
        INNER JOIN [tblCourse] co ON sco.[CourseID] = co.[CourseId] 
        WHERE co.[Status] = 1 AND sco.[CourseID] = @CourseID
    )";
                }
                //if (request.ClassID > 0)
                //{
                //    baseQuery += " AND scl.ClassID = @ClassID";
                //}
                //if (request.BoardID > 0)
                //{
                //    baseQuery += " AND sb.BoardID = @BoardID";
                //}
                //if (request.CourseID > 0)
                //{
                //    baseQuery += " AND sco.CourseID = @CourseID";
                //}
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND se.ExamTypeID = @ExamTypeID";
                }
                if (request.APID > 0)
                {
                    baseQuery += " AND sc.APID = @APID";
                }
                if (request.EventTypeID > 0)
                {
                    baseQuery += " AND s.EventTypeID = @EventTypeID";
                }
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery += " AND s.Status = 1";
                }
                // Parameters for the query
                var parameters = new
                {
                    ClassID = request.ClassID,
                    BoardID = request.BoardID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APID = request.APID,
                    EventTypeID = request.EventTypeID
                };

                // Fetch all matching records
                var mainResult = (await _connection.QueryAsync<dynamic>(baseQuery, parameters)).ToList();

                // Get current UTC date and time
                var utcNow = DateTime.UtcNow;

                // Convert UTC time to Indian Standard Time (IST)
                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var istNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istTimeZone);
                // Map results to response DTO
                var response = mainResult
                    .Select(item => new
                    {
                        item,
                        IsEvent1Valid = item.IsEvent1Valid == 1,
                        IsEvent2Valid = item.IsEvent2Valid == 1
                    })
                    .Where(item => item.IsEvent1Valid || item.IsEvent2Valid)
                    .Select(item => new StoryOfTheDayResponseDTO
                    {
                        StoryId = item.item.StoryId,
                        EventTypeID = item.item.EventTypeID,
                        modifiedby = item.item.modifiedby,
                        createdby = item.item.createdby,
                        eventtypename = item.item.eventtypename,
                        modifiedon = item.item.modifiedon,
                        createdon = item.item.createdon,
                        Status = item.item.Status,
                        EmployeeID = item.item.EmployeeID,
                        EmpFirstName = item.item.EmpFirstName,
                        SOTDCategories = GetListOfSOTDCategory(item.item.StoryId),
                        SOTDBoards = GetListOfSOTDBoards(item.item.StoryId),
                        SOTDClasses = GetListOfSOTDClass(item.item.StoryId),
                        SOTDCourses = GetListOfSOTDCourse(item.item.StoryId),
                        SOTDExamTypes = GetListOfSOTDExamType(item.item.StoryId),
                        Event1Posttime = item.IsEvent1Valid ? item.item.Event1Posttime : string.Empty,
                        Event1PostDate = item.IsEvent1Valid ? item.item.Event1PostDate : null,
                        Event1Image = item.IsEvent1Valid ? GetStoryOfTheDayFileById(item.item.Event1Image) : string.Empty,
                        Event2Posttime = item.IsEvent2Valid ? item.item.Event2Posttime : string.Empty,
                        Event2PostDate = item.IsEvent2Valid ? item.item.Event2PostDate : null,
                        Event2Image = item.IsEvent2Valid ? GetStoryOfTheDayFileById(item.item.Event2Image) : string.Empty
                    })
                    .ToList();

                // Total count before pagination
                int totalCount = response.Count;

                // Apply logical pagination
                var paginatedResponse = response
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if there are records
                if (paginatedResponse.Any())
                {
                    return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, "Records not found", new List<StoryOfTheDayResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, ex.Message, new List<StoryOfTheDayResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<StoryOfTheDayResponseDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                var response = new StoryOfTheDayResponseDTO();
                var query = @"
            SELECT 
                s.StoryId,
                s.EventTypeID,
                s.Event1PostDate,
                s.Event1Posttime,
                s.Event2PostDate,
                s.Event2Posttime,
                s.Event1Image,
                s.Event2Image,
                s.Status,
                s.APName,
                s.eventname,
                s.modifiedon,
                s.modifiedby,
                s.createdon,
                s.createdby,
                s.EmployeeID,
                ev.EventTypeName as eventtypename,
                e.EmpFirstName as EmpFirstName,
                CASE WHEN DATEADD(HOUR, -24, GETDATE()) <= CAST(s.Event1PostDate AS DATETIME) + CAST(s.Event1Posttime AS DATETIME) THEN 1 ELSE 0 END AS IsEvent1Valid,
                CASE WHEN DATEADD(HOUR, -24, GETDATE()) <= CAST(s.Event2PostDate AS DATETIME) + CAST(s.Event2Posttime AS DATETIME) THEN 1 ELSE 0 END AS IsEvent2Valid
            FROM tblSOTD s
            LEFT JOIN tblEmployee e ON s.EmployeeID = e.Employeeid
            LEFT JOIN tblSOTDEventtype ev ON s.EventTypeID = ev.EventTypeID
            WHERE s.StoryId = @StoryId;";

                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new { StoryId = id });

                if (storyOfTheDay != null)
                {
                    bool isEvent1Valid = storyOfTheDay.IsEvent1Valid == 1;
                    bool isEvent2Valid = storyOfTheDay.IsEvent2Valid == 1;

                    if (isEvent1Valid || isEvent2Valid)
                    {
                        if (isEvent1Valid)
                        {
                            response.Event1Posttime = storyOfTheDay.Event1Posttime;
                            response.Event1PostDate = storyOfTheDay.Event1PostDate;
                            response.Event1Image = !string.IsNullOrEmpty(storyOfTheDay.Event1Image) ? GetStoryOfTheDayFileById(storyOfTheDay.Event1Image) : string.Empty;
                        }

                        if (isEvent2Valid)
                        {
                            response.Event2Posttime = storyOfTheDay.Event2Posttime;
                            response.Event2PostDate = storyOfTheDay.Event2PostDate;
                            response.Event2Image = !string.IsNullOrEmpty(storyOfTheDay.Event2Image) ? GetStoryOfTheDayFileById(storyOfTheDay.Event2Image) : string.Empty;
                        }

                        response.StoryId = storyOfTheDay.StoryId;
                        response.EventTypeID = storyOfTheDay.EventTypeID;
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

                        return new ServiceResponse<StoryOfTheDayResponseDTO>(true, "Operation Successful", response, 200);
                    }
                    else
                    {
                        return new ServiceResponse<StoryOfTheDayResponseDTO>(false, "No event within the last 24 hours", new StoryOfTheDayResponseDTO(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<StoryOfTheDayResponseDTO>(false, "Operation Failed", new StoryOfTheDayResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StoryOfTheDayResponseDTO>(false, ex.Message, new StoryOfTheDayResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<StoryOfTheDayResponseDTO>>> GetStoryOfTheDayByPublishDateAndTime()
        {
            try
            {
                var responseList = new List<StoryOfTheDayResponseDTO>();

                // Get current UTC date and time
                var utcNow = DateTime.UtcNow;

                // Convert UTC time to Indian Standard Time (IST)
                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var istNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istTimeZone);

                // Calculate the threshold for the past 24 hours
                var ist24HoursAgo = istNow.AddHours(-24);

                // Retrieve all records that may be within the last 24 hours
                string query = @"
        SELECT 
            s.StoryId,
            s.EventTypeID,
            s.Event1PostDate,
            s.Event1Posttime,
            s.Event2PostDate,
            s.Event2Posttime,
            s.Filename1,
            s.Filename2,
            s.Status,
            s.APName,
            s.eventname,
            s.modifiedon,
            s.modifiedby,
            s.createdon,
            s.createdby,
            s.EmployeeID,
            ev.EventTypeName as eventtypename,
            e.EmpFirstName as EmpFirstName
        FROM tblSOTD s
        LEFT JOIN tblEmployee e ON s.EmployeeID = e.Employeeid
        LEFT JOIN tblSOTDEventtype ev ON s.EventTypeID = ev.EventTypeID";

                var storiesOfTheDay = await _connection.QueryAsync<dynamic>(query);

                foreach (var storyOfTheDay in storiesOfTheDay)
                {
                    bool isEvent1Valid = false;
                    bool isEvent2Valid = false;

                    // Check Event1 validity
                    if (storyOfTheDay.Event1PostDate != null && !string.IsNullOrEmpty(storyOfTheDay.Event1Posttime))
                    {
                        DateTime event1Date = storyOfTheDay.Event1PostDate;
                        TimeSpan event1Time = TimeSpan.Parse(storyOfTheDay.Event1Posttime);
                        DateTime event1DateTime = event1Date.Add(event1Time);

                        if (event1DateTime >= ist24HoursAgo && event1DateTime <= istNow)
                        {
                            isEvent1Valid = true;
                        }
                    }

                    // Check Event2 validity
                    if (storyOfTheDay.Event2PostDate != null && !string.IsNullOrEmpty(storyOfTheDay.Event2Posttime))
                    {
                        DateTime event2Date = storyOfTheDay.Event2PostDate;
                        TimeSpan event2Time = TimeSpan.Parse(storyOfTheDay.Event2Posttime);
                        DateTime event2DateTime = event2Date.Add(event2Time);

                        if (event2DateTime >= ist24HoursAgo && event2DateTime <= istNow)
                        {
                            isEvent2Valid = true;
                        }
                    }

                    if (isEvent1Valid || isEvent2Valid)
                    {
                        var response = new StoryOfTheDayResponseDTO
                        {
                            Event1Image = isEvent1Valid ? GetStoryOfTheDayFileById(storyOfTheDay.Event1Image) : string.Empty,
                            Event2Image = isEvent2Valid ? GetStoryOfTheDayFileById(storyOfTheDay.Event2Image) : string.Empty,
                            StoryId = storyOfTheDay.StoryId,
                            EventTypeID = storyOfTheDay.EventTypeID,
                            Event1Posttime = isEvent1Valid ? storyOfTheDay.Event1Posttime : string.Empty,
                            Event2Posttime = isEvent2Valid ? storyOfTheDay.Event2Posttime : string.Empty,
                            Event1PostDate = storyOfTheDay.Event1PostDate,
                            Event2PostDate = storyOfTheDay.Event2PostDate,
                            modifiedby = storyOfTheDay.modifiedby,
                            modifiedon = storyOfTheDay.modifiedon,
                            createdon = storyOfTheDay.createdon,
                            createdby = storyOfTheDay.createdby,
                            eventtypename = storyOfTheDay.eventtypename,
                            Status = storyOfTheDay.Status,
                            EmployeeID = storyOfTheDay.EmployeeID,
                            EmpFirstName = storyOfTheDay.EmpFirstName,
                            SOTDBoards = GetListOfSOTDBoards(storyOfTheDay.StoryId),
                            SOTDCategories = GetListOfSOTDCategory(storyOfTheDay.StoryId),
                            SOTDClasses = GetListOfSOTDClass(storyOfTheDay.StoryId),
                            SOTDCourses = GetListOfSOTDCourse(storyOfTheDay.StoryId),
                            SOTDExamTypes = GetListOfSOTDExamType(storyOfTheDay.StoryId)
                        };

                        responseList.Add(response);
                    }
                }

                if (responseList.Count > 0)
                {
                    return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(true, "Operation Successful", responseList, 200);
                }
                else
                {
                    return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, "No event within the last 24 hours", [], 404);
                }
            }
            catch (SqlException sqlEx)
            {
                // Log sqlEx for detailed error analysis
                return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, sqlEx.Message, new List<StoryOfTheDayResponseDTO>(), 500);
            }
            catch (Exception ex)
            {
                // Log ex for detailed error analysis
                return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, ex.Message, new List<StoryOfTheDayResponseDTO>(), 500);
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
        public async Task<ServiceResponse<List<Category>>> GetCategoryList()
        {
            try
            {
                var query = "SELECT * FROM [tblCategory];";
                var eventTypes = await _connection.QueryAsync<Category>(query);
                if (eventTypes != null)
                {
                    return new ServiceResponse<List<Category>>(true, "Operation Successful", eventTypes.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Category>>(false, "Opertion Failed", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Category>>(false, ex.Message, [], 500);
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
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
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
                    var insertquery = @"INSERT INTO [tblSOTDCategory] ([APID], [SOTDID])
                          VALUES (@APID, @SOTDID);";
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
                var insertquery = @"INSERT INTO [tblSOTDCategory] ([APID], [SOTDID])
                          VALUES (@APID, @SOTDID);";
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
                    var insertquery = @"INSERT INTO [tblSOTDClass] ([SOTDID], [ClassID])
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
                var insertquery = @"INSERT INTO [tblSOTDClass] ([SOTDID], [ClassID])
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
            string query = "SELECT COUNT(*) FROM [tblSOTDBoard] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDBoard]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDBoard] ([SOTDID], [BoardID])
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
                var insertquery = @"INSERT INTO [tblSOTDBoard] ([SOTDID], [BoardID])
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
            string query = "SELECT COUNT(*) FROM [tblSOTDCourse] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDCourse]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDCourse] ([SOTDID], [CourseID])
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
                var insertquery = @"INSERT INTO [tblSOTDCourse] ([SOTDID], [CourseID])
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
            string query = "SELECT COUNT(*) FROM [tblSOTDExamType] WHERE [SOTDID] = @SOTDID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SOTDID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSOTDExamType]
                          WHERE [SOTDID] = @SOTDID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SOTDID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblSOTDExamType] ([SOTDID], [ExamTypeID])
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
                var insertquery = @"INSERT INTO [tblSOTDExamType] ([SOTDID], [ExamTypeID])
                          VALUES (@SOTDID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<SOTDBoardResponse> GetListOfSOTDBoards(int SOTDID)
        {
            var query = @"
    SELECT 
        b.tblSOTDBoardID,
        b.SOTDID,
        b.BoardID,
        bc.BoardName as Name
    FROM 
        [tblSOTDBoard] b
    LEFT JOIN 
        tblBoard bc ON b.BoardID = bc.BoardID
    WHERE 
        b.SOTDID = @SOTDID
        AND bc.Status = 1;"; // Ensure board is active

            var boardData = _connection.Query<SOTDBoardResponse>(query, new { SOTDID });
            return boardData != null ? boardData.AsList() : new List<SOTDBoardResponse>();
        }
        private List<SOTDCategoryResponse> GetListOfSOTDCategory(int SOTDID)
        {
            var query = @"
        SELECT 
            c.SOTDCategoryID,
            c.SOTDID,
            c.APID,
            cap.APName as APIDName
        FROM 
            [tblSOTDCategory] c
        LEFT JOIN 
            tblCategory cap ON c.APID = cap.APId
        WHERE 
            c.SOTDID = @SOTDID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<SOTDCategoryResponse>(query, new { SOTDID });
            return data != null ? data.AsList() : [];
        }
        private List<SOTDClassResponse> GetListOfSOTDClass(int SOTDID)
        {
            var query = @"
    SELECT 
        cl.tblSOTDClassID,
        cl.SOTDID,
        cl.ClassID,
        cc.ClassName as Name
    FROM 
        [tblSOTDClass] cl
    LEFT JOIN 
        tblClass cc ON cl.ClassID = cc.ClassID
    WHERE 
        cl.SOTDID = @SOTDID
        AND cc.Status = 1;"; // Ensure class is active

            var data = _connection.Query<SOTDClassResponse>(query, new { SOTDID });
            return data != null ? data.AsList() : new List<SOTDClassResponse>();
        }
        private List<SOTDCourseResponse> GetListOfSOTDCourse(int SOTDID)
        {
            var query = @"
    SELECT 
        co.SOTDCourseID,
        co.SOTDID,
        co.CourseID,
        cn.CourseName as Name
    FROM 
        [tblSOTDCourse] co
    LEFT JOIN 
        tblCourse cn ON co.CourseID = cn.CourseID
    WHERE 
        co.SOTDID = @SOTDID
        AND cn.Status = 1;"; // Ensure course is active

            var data = _connection.Query<SOTDCourseResponse>(query, new { SOTDID });
            return data != null ? data.AsList() : new List<SOTDCourseResponse>();
        }
        private List<SOTDExamTypeResponse> GetListOfSOTDExamType(int SOTDID)
        {
            var query = @"
        SELECT 
            et.SOTDExamTypeID,
            et.SOTDID,
            et.ExamTypeID,
            ex.ExamTypeName as Name
        FROM 
            [tblSOTDExamType] et
        LEFT JOIN 
            tblExamType ex ON et.ExamTypeID = ex.ExamTypeID
        WHERE 
            et.SOTDID = @SOTDID;";
            var data = _connection.Query<SOTDExamTypeResponse>(query, new { SOTDID });
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
