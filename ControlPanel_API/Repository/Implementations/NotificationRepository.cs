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
                        PDF = PDFUpload(request.PDF ??= string.Empty),
                        status = true,
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        EmployeeID = request.EmployeeID
                    };
                    string insertQuery = @"
                    INSERT INTO [tblNbNotification] (NotificationTitle, PDF, status, createdon, createdby, EmployeeID)
                    VALUES (@NotificationTitle, @PDF, @status, @createdon, @createdby, @EmployeeID);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    int notificationId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, newNotification);
                    if (notificationId > 0)
                    {
                        var data = await AddUpdateNotificationDetailsMaster(request.NotificationDetails, notificationId);
                        var data1 = await AddUpdateNotificationLinkMaster(request.NotificationLinkMasters, notificationId);
                        int category = NBCategoryMapping(request.NbNotificationCategories ??= ([]), notificationId);
                        int classes = NBClassMapping(request.NbNotificationClasses ??= ([]), notificationId);
                        int board = NBBoardMapping(request.NbNotificationBoards ??= ([]), notificationId);
                        int course = NBCourseMapping(request.NbNotificationCourses ??= ([]), notificationId);
                        int exam = NBExamTypeMapping(request.NbNotificationExamTypes ??= ([]), notificationId);
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
                        PDF = PDFUpload(request.PDF ??= string.Empty),
                        status = request.status,
                        modifiedon = DateTime.Now,
                        modifiedby = request.modifiedby,
                        EmployeeID = request.EmployeeID,
                        NBNotificationID = request.NBNotificationID
                    };
                    string updateQuery = @"
                    UPDATE [tblNbNotification]
                    SET NotificationTitle = @NotificationTitle,
                    PDF = @PDF,
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
        //    public async Task<ServiceResponse<List<NotificationResponseDTO>>> GetAllNotificationsList(NotificationsListDTO request)
        //    {
        //        try
        //        {
        //            // Base query to fetch all matching records
        //            string baseQuery = @"
        //    SELECT 
        //        n.NBNotificationID, 
        //        n.NotificationTitle, 
        //        n.PDF, 
        //        n.status, 
        //        n.createdon, 
        //        n.createdby, 
        //        n.modifiedon, 
        //        n.modifiedby, 
        //        n.EmployeeID,
        //        e.EmpFirstName as EmpFirstName
        //    FROM tblNbNotification n
        //    LEFT JOIN tblEmployee e ON n.EmployeeID = e.Employeeid
        //    WHERE 1=1";

        //            // Applying filters
        //            if (request.APId > 0)
        //            {
        //                baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationCategory] WHERE [APID] = @APId)";
        //            }
        //            //if (request.BoardID > 0)
        //            //{
        //            //    baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationBoard] WHERE [BoardID] = @BoardID)";
        //            //}
        //            //if (request.ClassID > 0)
        //            //{
        //            //    baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationClass] WHERE [ClassID] = @ClassID)";
        //            //}
        //            //if (request.CourseID > 0)
        //            //{
        //            //    baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationCourse] WHERE [CourseID] = @CourseID)";
        //            //}
        //            if (request.BoardID > 0)
        //            {
        //                baseQuery += @"
        //AND n.NBNotificationID IN (
        //    SELECT nb.[NBNotificationID] 
        //    FROM [tblNbNotificationBoard] nb 
        //    INNER JOIN [tblBoard] b ON nb.[BoardID] = b.[BoardId] 
        //    WHERE b.[Status] = 1 AND nb.[BoardID] = @BoardID
        //)";
        //            }
        //            if (request.ClassID > 0)
        //            {
        //                baseQuery += @"
        //AND n.NBNotificationID IN (
        //    SELECT nc.[NBNotificationID] 
        //    FROM [tblNbNotificationClass] nc 
        //    INNER JOIN [tblClass] c ON nc.[ClassID] = c.[ClassId] 
        //    WHERE c.[Status] = 1 AND nc.[ClassID] = @ClassID
        //)";
        //            }
        //            if (request.CourseID > 0)
        //            {
        //                baseQuery += @"
        //AND n.NBNotificationID IN (
        //    SELECT nco.[NBNotificationID] 
        //    FROM [tblNbNotificationCourse] nco 
        //    INNER JOIN [tblCourse] co ON nco.[CourseID] = co.[CourseId] 
        //    WHERE co.[Status] = 1 AND nco.[CourseID] = @CourseID
        //)";
        //            }
        //            if (request.ExamTypeID > 0)
        //            {
        //                baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationExamType] WHERE [ExamTypeID] = @ExamTypeID)";
        //            }

        //            // Parameters for the query
        //            var parameters = new
        //            {
        //                APId = request.APId,
        //                BoardID = request.BoardID,
        //                ClassID = request.ClassID,
        //                CourseID = request.CourseID,
        //                ExamTypeID = request.ExamTypeID
        //            };

        //            // Fetch all matching records
        //            var mainResult = (await _connection.QueryAsync<dynamic>(baseQuery, parameters)).ToList();

        //            // Map results to response DTO
        //            var response = mainResult.Select(item => new NotificationResponseDTO
        //            {
        //                NBNotificationID = item.NBNotificationID,
        //                NotificationTitle = item.NotificationTitle,
        //                PDF = GetPDF(item.PDF),
        //                status = item.status,
        //                createdon = item.createdon,
        //                createdby = item.createdby,
        //                modifiedon = item.modifiedon,
        //                modifiedby = item.modifiedby,
        //                EmployeeID = item.EmployeeID,
        //                EmpFirstName = item.EmpFirstName,
        //                NbNotificationCategories = GetListOfNBCategory(item.NBNotificationID),
        //                NbNotificationBoards = GetListOfNBBoards(item.NBNotificationID),
        //                NbNotificationClasses = GetListOfNBClass(item.NBNotificationID),
        //                NbNotificationCourses = GetListOfNBCourse(item.NBNotificationID),
        //                NbNotificationExamTypes = GetListOfNBExamType(item.NBNotificationID),
        //                NotificationDetails = GetListOfNotificationDetails(item.NBNotificationID),
        //                NotificationLinkMasters = GetListOfNotificationLink(item.NBNotificationID)
        //            }).ToList();

        //            // Total count before pagination
        //            int totalCount = response.Count;

        //            // Apply logical pagination
        //            var paginatedResponse = response
        //                .Skip((request.PageNumber - 1) * request.PageSize)
        //                .Take(request.PageSize)
        //                .ToList();

        //            // Check if there are records
        //            if (paginatedResponse.Any())
        //            {
        //                return new ServiceResponse<List<NotificationResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
        //            }
        //            else
        //            {
        //                return new ServiceResponse<List<NotificationResponseDTO>>(false, "Records not found", new List<NotificationResponseDTO>(), 404);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ServiceResponse<List<NotificationResponseDTO>>(false, ex.Message, new List<NotificationResponseDTO>(), 500);
        //        }
        //    }
        public async Task<ServiceResponse<List<NotificationResponseDTO>>> GetAllNotificationsList(NotificationsListDTO request)
        {
            try
            {
                // Fetch employee's role information
                var employeeRoleQuery = "SELECT e.RoleID, r.RoleCode FROM tblEmployee e INNER JOIN tblRole r ON e.RoleID = r.RoleID WHERE e.Employeeid = @EmployeeID";
                var employeeRole = await _connection.QuerySingleOrDefaultAsync<dynamic>(employeeRoleQuery, new { EmployeeID = request.EmployeeId });

                // Determine if the employee is Admin or SuperAdmin
                bool isAdminOrSuperAdmin = employeeRole != null && (employeeRole.RoleCode == "AD" || employeeRole.RoleCode == "SA");

                // Base query to fetch all matching records
                string baseQuery = @"
        SELECT 
            n.NBNotificationID, 
            n.NotificationTitle, 
            n.PDF, 
            n.status, 
            n.createdon, 
            n.createdby, 
            n.modifiedon, 
            n.modifiedby, 
            n.EmployeeID,
            e.EmpFirstName as EmpFirstName
        FROM tblNbNotification n
        LEFT JOIN tblEmployee e ON n.EmployeeID = e.Employeeid
        WHERE 1=1";

                // Applying filters
                if (request.APId > 0)
                {
                    baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationCategory] WHERE [APID] = @APId)";
                }
                if (request.BoardID > 0)
                {
                    baseQuery += @"
            AND n.NBNotificationID IN (
                SELECT nb.[NBNotificationID] 
                FROM [tblNbNotificationBoard] nb 
                INNER JOIN [tblBoard] b ON nb.[BoardID] = b.[BoardId] 
                WHERE b.[Status] = 1 AND nb.[BoardID] = @BoardID
            )";
                }
                if (request.ClassID > 0)
                {
                    baseQuery += @"
            AND n.NBNotificationID IN (
                SELECT nc.[NBNotificationID] 
                FROM [tblNbNotificationClass] nc 
                INNER JOIN [tblClass] c ON nc.[ClassID] = c.[ClassId] 
                WHERE c.[Status] = 1 AND nc.[ClassID] = @ClassID
            )";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += @"
            AND n.NBNotificationID IN (
                SELECT nco.[NBNotificationID] 
                FROM [tblNbNotificationCourse] nco 
                INNER JOIN [tblCourse] co ON nco.[CourseID] = co.[CourseId] 
                WHERE co.[Status] = 1 AND nco.[CourseID] = @CourseID
            )";
                }
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND n.NBNotificationID IN (SELECT [NBNotificationID] FROM [tblNbNotificationExamType] WHERE [ExamTypeID] = @ExamTypeID)";
                }

                // Add condition for non-Admin users to see only active records
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery += " AND n.status = 1";
                }

                // Parameters for the query
                var parameters = new
                {
                    APId = request.APId,
                    BoardID = request.BoardID,
                    ClassID = request.ClassID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID
                };

                // Fetch all matching records
                var mainResult = (await _connection.QueryAsync<dynamic>(baseQuery, parameters)).ToList();

                // Map results to response DTO
                var response = mainResult.Select(item => new NotificationResponseDTO
                {
                    NBNotificationID = item.NBNotificationID,
                    NotificationTitle = item.NotificationTitle,
                    PDF = GetPDF(item.PDF),
                    status = item.status,
                    createdon = item.createdon,
                    createdby = item.createdby,
                    modifiedon = item.modifiedon,
                    modifiedby = item.modifiedby,
                    EmployeeID = item.EmployeeID,
                    EmpFirstName = item.EmpFirstName,
                    NbNotificationCategories = GetListOfNBCategory(item.NBNotificationID),
                    NbNotificationBoards = GetListOfNBBoards(item.NBNotificationID),
                    NbNotificationClasses = GetListOfNBClass(item.NBNotificationID),
                    NbNotificationCourses = GetListOfNBCourse(item.NBNotificationID),
                    NbNotificationExamTypes = GetListOfNBExamType(item.NBNotificationID),
                    NotificationDetails = GetListOfNotificationDetails(item.NBNotificationID),
                    NotificationLinkMasters = GetListOfNotificationLink(item.NBNotificationID)
                }).ToList();

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
                    return new ServiceResponse<List<NotificationResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<NotificationResponseDTO>>(false, "Records not found", new List<NotificationResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationResponseDTO>>(false, ex.Message, new List<NotificationResponseDTO>(), 500);
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
            n.PDF, 
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
                    response.PDF = GetPDF(data.PDF);
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
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetNotificationById(id);

                if (data.Data != null)
                {
                    data.Data.status = !data.Data.status;

                    string sql = "UPDATE tblNbNotification SET status = @Status WHERE NBNotificationID = @NBNotificationID";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { data.Data.status, NBNotificationID = id });
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
            string fileExtension = IsPdf(imageData) == true ? ".pdf" : string.Empty;
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
        private bool IsPdf(byte[] fileData)
        {
            return fileData.Length > 4 &&
                   fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46;
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
            var boardQuery = @"
    SELECT 
        nb.NbNotificationBoardId, 
        nb.NBNotificationID, 
        nb.BoardID, 
        b.BoardName as Name
    FROM tblNbNotificationBoard nb
    LEFT JOIN tblBoard b ON nb.BoardID = b.BoardID
    WHERE nb.NBNotificationID = @NotificationId
      AND b.Status = 1;"; // Ensure board is active

            var boardData = _connection.Query<NbNotificationBoardResponse>(boardQuery, new { NotificationId = notificationId });
            return boardData != null ? boardData.AsList() : new List<NbNotificationBoardResponse>();
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
    WHERE nc.NBNotificationID = @NotificationId
      AND c.Status = 1;"; // Ensure class is active

            var data = _connection.Query<NbNotificationClassResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : new List<NbNotificationClassResponse>();
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
    WHERE nc.NBNotificationID = @NotificationId
      AND c.Status = 1;"; // Ensure course is active

            var data = _connection.Query<NbNotificationCourseResponse>(query, new { NotificationId = notificationId });
            return data != null ? data.AsList() : new List<NbNotificationCourseResponse>();
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
