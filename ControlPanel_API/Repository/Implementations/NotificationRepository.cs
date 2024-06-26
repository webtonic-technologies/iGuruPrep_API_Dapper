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
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string? _connectionString;

        public NotificationRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                if (request.NBNotificationID == 0)
                {
                    var newNotification = new Notification
                    {
                        NotificationTitle = request.NotificationTitle,
                        PathURL = PDFUpload(request.PathURL ??= string.Empty),
                        status = true,
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        EmployeeID = request.EmployeeID
                    };
                    string insertQuery = @"
                    INSERT INTO [tblNbNotification] (NotificationTitle, PathURL, status, createdon, createdby, EmployeeID)
                    VALUES (@NotificationTitle, @PathURL, @status, @createdon, @createdby, @EmployeeID);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    int notificationId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, newNotification);
                    if (notificationId > 0)
                    {
                        var data = await AddUpdateNotificationDetailsMaster(request.NotificationDetails, request.NBNotificationID);
                        var data1 = await AddUpdateNotificationLinkMaster(request.NotificationLinkMasters, request.NBNotificationID);
                        int category = NBCategoryMapping(request.NbNotificationCategories ??= ([]), request.NBNotificationID);
                        int classes = NBClassMapping(request.NbNotificationClasses ??= ([]), request.NBNotificationID);
                        int board = NBBoardMapping(request.NbNotificationBoards ??= ([]), request.NBNotificationID);
                        int course = NBCourseMapping(request.NbNotificationCourses ??= ([]), request.NBNotificationID);
                        int exam = NBExamTypeMapping(request.NbNotificationExamTypes ??= ([]), request.NBNotificationID);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && data > 0 && data1 > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Notice Board Notification Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    var newNotification = new Notification
                    {
                        NotificationTitle = request.NotificationTitle,
                        PathURL = PDFUpload(request.PathURL ??= string.Empty),
                        status = request.status,
                        modifiedon = DateTime.Now,
                        modifiedby = request.modifiedby,
                        EmployeeID = request.EmployeeID,
                        NBNotificationID = request.NBNotificationID
                    };
                    string updateQuery = @"
                    UPDATE [tblNbNotification]
                    SET NotificationTitle = @NotificationTitle,
                    PathURL = @PathURL,
                    status = @status,
                    modifiedon = @modifiedon,
                    modifiedby = @modifiedby,
                    EmployeeID = @EmployeeID
                    WHERE NBNotificationID = @NBNotificationID;";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newNotification);
                    if (rowsAffected > 0)
                    {
                        var data = await AddUpdateNotificationDetailsMaster(request.NotificationDetails, request.NBNotificationID);
                        var data1 = await AddUpdateNotificationLinkMaster(request.NotificationLinkMasters, request.NBNotificationID);
                        int category = NBCategoryMapping(request.NbNotificationCategories ??= ([]), request.NBNotificationID);
                        int classes = NBClassMapping(request.NbNotificationClasses ??= ([]), request.NBNotificationID);
                        int board = NBBoardMapping(request.NbNotificationBoards ??= ([]), request.NBNotificationID);
                        int course = NBCourseMapping(request.NbNotificationCourses ??= ([]), request.NBNotificationID);
                        int exam = NBExamTypeMapping(request.NbNotificationExamTypes ??= ([]), request.NBNotificationID);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && data > 0 && data1 > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Notice Board Notification Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<NotificationResponseDTO>>> GetAllNotificationsList(NotificationsListDTO request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblNbNotification]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                var notificationIds = new HashSet<int>();

                // Define the queries
                string categoriesQuery = @"SELECT [NBNotificationID] FROM [tblNbNotificationCategory] WHERE [APID] = @APId";
                string boardsQuery = @"SELECT [NBNotificationID] FROM [tblNbNotificationBoard] WHERE [BoardID] = @BoardID";
                string classesQuery = @"SELECT [NBNotificationID] FROM [tblNbNotificationClass] WHERE [ClassID] = @ClassID";
                string coursesQuery = @"SELECT [NBNotificationID] FROM [tblNbNotificationCourse] WHERE [CourseID] = @CourseID";
                string examsQuery = @"SELECT [NBNotificationID] FROM [tblNbNotificationExamType] WHERE [ExamTypeID] = @ExamTypeID";
                // string subjectQuery = @"SELECT [bookID] FROM [tbllibrarySubject] WHERE [SubjectID] = @SubjectID";

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

                // Wait for all tasks to complete
                var results = await Task.WhenAll(categoryTask, boardTask, classTask, courseTask, examTask);

                // Add all results to the HashSet to ensure uniqueness
                foreach (var result in results)
                {
                    foreach (var id in result)
                    {
                        notificationIds.Add(id);
                    }
                }

                // Prepare the list of IDs for the final query
                var parameters = new { Ids = notificationIds.ToList() };

                // Main query to fetch magazine details
                string mainQuery = @"
        SELECT 
            n.NBNotificationID, 
            n.NotificationTitle, 
            n.PathURL, 
            n.status, 
            n.createdon, 
            n.createdby, 
            n.modifiedon, 
            n.modifiedby, 
            n.EmployeeID,
            e.EmpFirstName as EmpFirstName
        FROM tblNbNotification n
        LEFT JOIN tblEmployee e ON n.EmployeeID = e.Employeeid
        WHERE n.NBNotificationID IN @Ids";

                var data = await _connection.QueryAsync<dynamic>(mainQuery, parameters);

                var response = data.Select(item => new NotificationResponseDTO
                {
                    NbNotificationCategories = GetListOfNBCategory(item.NBNotificationID),
                    NbNotificationBoards = GetListOfNBBoards(item.NBNotificationID),
                    NbNotificationClasses = GetListOfNBClass(item.NBNotificationID),
                    NbNotificationCourses = GetListOfNBCourse(item.NBNotificationID),
                    NbNotificationExamTypes = GetListOfNBExamType(item.NBNotificationID),
                    NotificationTitle = item.NotificationTitle,
                    PathURL = GetPDF(item.PathURL ??= string.Empty),
                    status = item.status,
                    createdon = item.createdon,
                    createdby = item.createdby,
                    EmployeeID = item.EmployeeID,
                    EmpFirstName = item.EmpFirstName,
                    modifiedby = item.modifiedby,
                    modifiedon = item.modifiedon,
                    NBNotificationID = item.NBNotificationID,
                    NotificationDetails = GetListOfNotificationDetails(item.NBNotificationID),
                    NotificationLinkMasters = GetListOfNotificationLink(item.NBNotificationID)
                }).ToList();
                var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
              .Take(request.PageSize)
              .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<NotificationResponseDTO>>(true, "Records found", paginatedList, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<NotificationResponseDTO>>(false, "Records not found", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<NotificationResponseDTO>> GetNotificationById(int NotificationId)
        {
            try
            {
                var response = new NotificationResponseDTO();

                string selectQuery = @"
        SELECT 
            n.NBNotificationID, 
            n.NotificationTitle, 
            n.PathURL, 
            n.status, 
            n.createdon, 
            n.createdby, 
            n.modifiedon, 
            n.modifiedby, 
            n.EmployeeID,
            e.EmpFirstName as EmpFirstName
        FROM tblNbNotification n
        LEFT JOIN tblEmployee e ON n.EmployeeID = e.Employeeid
        WHERE n.NBNotificationID = @NotificationId";
                var data = await _connection.QuerySingleOrDefaultAsync<dynamic>(selectQuery, new { NotificationId });
                if (data != null)
                    {
                    string selectNLinkQuery = @"
            SELECT *
            FROM tblNbNotificationLink
            WHERE NBNLID = @NotificationId";
                    var links = await _connection.QueryAsync<NotificationLinkMaster>(selectNLinkQuery, new { NotificationId });
                    if (links != null)
                        response.NotificationLinkMasters = links.AsList();
                    string selectNDetailQuery = @"
            SELECT *
            FROM tblNbNotificationDetail
            WHERE NBNID = @NotificationId";
                    var details = await _connection.QueryAsync<NotificationDetail>(selectNDetailQuery, new { NotificationId });
                    if (details != null)
                        response.NotificationDetails = details.AsList();

                    response.NbNotificationCategories = GetListOfNBCategory(NotificationId);
                    response.NbNotificationBoards = GetListOfNBBoards(NotificationId);
                    response.NbNotificationClasses = GetListOfNBClass(NotificationId);
                    response.NbNotificationCourses = GetListOfNBCourse(NotificationId);
                    response.NbNotificationExamTypes = GetListOfNBExamType(NotificationId);
                    response.NotificationTitle = data.NotificationTitle;
                    response.PathURL = GetPDF(data.PathURL ??= string.Empty);
                    response.status = data.status;
                    response.createdon = data.createdon;
                    response.createdby = data.createdby;
                    response.EmployeeID = data.EmployeeID;
                    response.EmpFirstName = data.EmpFirstName;
                    response.modifiedby = data.modifiedby;
                    response.modifiedon = data.modifiedon;
                    response.NBNotificationID = data.NBNotificationID;

                    return new ServiceResponse<NotificationResponseDTO>(true, "Records Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<NotificationResponseDTO>(true, "Records Not Found", new NotificationResponseDTO(), 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationResponseDTO>(false, ex.Message, new NotificationResponseDTO(), 500);
            }
        }
        private async Task<int> AddUpdateNotificationDetailsMaster(List<NotificationDetail>? request, int notificationId)
        {
            int rowsAffected = 0;
            string insertQuery = @"
            INSERT INTO tblNbNotificationDetail (NBNID, NBNotificationDetail)
            VALUES (@NBNID, @NBNotificationDetail)";

            string updateQuery = @"
            UPDATE tblNbNotificationDetail
            SET NBNID = @NBNID,
                NBNotificationDetail = @NBNotificationDetail
            WHERE NBNotificationDetailid = @NBNotificationDetailid";
            if (request != null)
            {
                foreach (var data in request)
                {
                    var newNDetails = new NotificationDetail
                    {
                        NBNID = notificationId,
                        NBNotificationDetail = data.NBNotificationDetail,
                        NBNotificationDetailid = data.NBNotificationDetailid,
                    };
                    if (data.NBNotificationDetailid == 0)
                    {
                        rowsAffected = await _connection.ExecuteAsync(insertQuery, newNDetails);
                    }
                    else
                    {
                        rowsAffected = await _connection.ExecuteAsync(updateQuery, newNDetails);
                    }
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> AddUpdateNotificationLinkMaster(List<NotificationLinkMaster>? request, int notificationId)
        {
            int rowsAffected = 0;

            string insertQuery = @"
            INSERT INTO tblNbNotificationLink (NBNLID, NotificationLink)
            VALUES (@NBNLID, @NotificationLink)";

            string updateQuery = @"
            UPDATE tblNbNotificationLink
            SET NBNLID = @NBNLID,
                NotificationLink = @NotificationLink
            WHERE NBNotificationLinkId = @NBNotificationLinkId";
            if (request != null)
            {
                foreach (var data in request)
                {
                    var newNLink = new NotificationLinkMaster
                    {
                        NBNLID = notificationId,
                        NotificationLink = data.NotificationLink,
                        NBNotificationLinkId = data.NBNotificationLinkId
                    };
                    if (data.NBNotificationLinkId == 0)
                    {
                        rowsAffected = await _connection.ExecuteAsync(insertQuery, newNLink);
                    }
                    else
                    {
                        rowsAffected = await _connection.ExecuteAsync(updateQuery, newNLink);
                    }
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
        private string PDFUpload(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "NBNotification");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = Guid.NewGuid().ToString() + ".pdf";
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "NBNotification", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private int NBCategoryMapping(List<NbNotificationCategory> request, int notificationId)
        {
            foreach (var data in request)
            {
                data.NBNotificationID = notificationId;
            }
            string query = "SELECT COUNT(*) FROM [tblNbNotificationCategory] WHERE [NBNotificationID] = @NBNotificationID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBNotificationID = notificationId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNbNotificationCategory]
                          WHERE [NBNotificationID] = @NBNotificationID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NBNotificationID = notificationId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNbNotificationCategory] ([APID], [NBNotificationID])
                          VALUES (@APID, @NBNotificationID);";
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
                var insertquery = @"INSERT INTO [tblNbNotificationCategory] ([APID], [NBNotificationID])
                          VALUES (@APID, @NBNotificationID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int NBClassMapping(List<NbNotificationClass> request, int notificationId)
        {
            foreach (var data in request)
            {
                data.NBNotificationID = notificationId;
            }
            string query = "SELECT COUNT(*) FROM [tblNbNotificationClass] WHERE [NBNotificationID] = @NBNotificationID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBNotificationID = notificationId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNbNotificationClass]
                          WHERE [NBNotificationID] = @NBNotificationID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NBNotificationID = notificationId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNbNotificationClass] ([NBNotificationID], [ClassID])
                          VALUES (@NBNotificationID, @ClassID);";
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
                var insertquery = @"INSERT INTO [tblNbNotificationClass] ([NBNotificationID], [ClassID])
                          VALUES (@NBNotificationID, @ClassID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int NBBoardMapping(List<NbNotificationBoard> request, int notificationId)
        {
            foreach (var data in request)
            {
                data.NBNotificationID = notificationId;
            }
            string query = "SELECT COUNT(*) FROM [tblNbNotificationBoard] WHERE [NBNotificationID] = @NBNotificationID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBNotificationID = notificationId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNbNotificationBoard]
                          WHERE [NBNotificationID] = @NBNotificationID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NBNotificationID = notificationId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNbNotificationBoard] ([NBNotificationID], [BoardID])
                          VALUES (@NBNotificationID, @BoardID);";
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
                var insertquery = @"INSERT INTO [tblNbNotificationBoard] ([NBNotificationID], [BoardID])
                          VALUES (@NBNotificationID, @BoardID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int NBCourseMapping(List<NbNotificationCourse> request, int notificationId)
        {
            foreach (var data in request)
            {
                data.NBNotificationID = notificationId;
            }
            string query = "SELECT COUNT(*) FROM [tblNbNotificationCourse] WHERE [NBNotificationID] = @NBNotificationID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBNotificationID = notificationId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNbNotificationCourse]
                          WHERE [NBNotificationID] = @NBNotificationID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NBNotificationID = notificationId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNbNotificationCourse] ([NBNotificationID], [CourseID])
                          VALUES (@NBNotificationID, @CourseID);";
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
                var insertquery = @"INSERT INTO [tblNbNotificationCourse] ([NBNotificationID], [CourseID])
                          VALUES (@NBNotificationID, @CourseID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int NBExamTypeMapping(List<NbNotificationExamType> request, int notificationId)
        {
            foreach (var data in request)
            {
                data.NBNotificationID = notificationId;
            }
            string query = "SELECT COUNT(*) FROM [tblNbNotificationExamType] WHERE [NBNotificationID] = @NBNotificationID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBNotificationID = notificationId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNbNotificationExamType]
                          WHERE [NBNotificationID] = @NBNotificationID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NBNotificationID = notificationId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNbNotificationExamType] ([NBNotificationID], [ExamTypeID])
                          VALUES (@NBNotificationID, @ExamTypeID);";
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
                var insertquery = @"INSERT INTO [tblNbNotificationExamType] ([NBNotificationID], [ExamTypeID])
                          VALUES (@NBNotificationID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<NbNotificationBoardResponse> GetListOfNBBoards(int notificationId)
        {
            var boardquery = @"
        SELECT 
            nb.NbNotificationBoardId, 
            nb.NBNotificationID, 
            nb.BoardID, 
            b.BoardName as Name
        FROM tblNbNotificationBoard nb
        LEFT JOIN tblBoard b ON nb.BoardID = b.BoardID
        WHERE nb.NBNotificationID = @NotificationId;";
            var boardData = _connection.Query<NbNotificationBoardResponse>(boardquery, new { NotificationId = notificationId });
            return boardData != null ? boardData.AsList() : [];
        }
        private List<NbNotificationCategoryResponse> GetListOfNBCategory(int notificationId)
        {
            var query = @"
        SELECT 
            nc.NbNotificationCategoryId, 
            nc.NBNotificationID, 
            nc.APID, 
            c.APName as Name
        FROM tblNbNotificationCategory nc
        LEFT JOIN tblCategory c ON nc.APID = c.APId
        WHERE nc.NBNotificationID = @NotificationId;";
            var data = _connection.Query<NbNotificationCategoryResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationClassResponse> GetListOfNBClass(int notificationId)
        {
            var query = @"
        SELECT 
            nc.NbNotificationClassId, 
            nc.NBNotificationID, 
            nc.ClassID, 
            c.ClassName as Name
        FROM tblNbNotificationClass nc
        LEFT JOIN tblClass c ON nc.ClassID = c.ClassID
        WHERE nc.NBNotificationID = @NotificationId;";
            var data = _connection.Query<NbNotificationClassResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationCourseResponse> GetListOfNBCourse(int notificationId)
        {
            var query = @"
        SELECT 
            nc.NbNotificationCourseId, 
            nc.NBNotificationID, 
            nc.CourseID, 
            c.CourseName as Name
        FROM tblNbNotificationCourse nc
        LEFT JOIN tblCourse c ON nc.CourseID = c.CourseID
        WHERE nc.NBNotificationID = @NotificationId;";
            var data = _connection.Query<NbNotificationCourseResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationExamTypeResponse> GetListOfNBExamType(int notificationId)
        {
            var query = @"
        SELECT 
            ne.NbNotificationExamTypeId, 
            ne.NBNotificationID, 
            ne.ExamTypeID, 
            et.ExamTypeName as Name
        FROM tblNbNotificationExamType ne
        LEFT JOIN tblExamType et ON ne.ExamTypeID = et.ExamTypeID
        WHERE ne.NBNotificationID = @NotificationId;";
            var data = _connection.Query<NbNotificationExamTypeResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NotificationDetail> GetListOfNotificationDetails(int notificationId)
        {
            string query = @"SELECT * FROM tblNbNotificationDetail WHERE NBNID = @NotificationId";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NotificationDetail>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NotificationLinkMaster> GetListOfNotificationLink(int notificationId)
        {
            string query = @"SELECT * FROM tblNbNotificationLink WHERE NBNLID = @NotificationId";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NotificationLinkMaster>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : [];
        }
    }
}
