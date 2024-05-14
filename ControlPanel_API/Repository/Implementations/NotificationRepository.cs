using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public NotificationRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
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
                        EmployeeID = request.EmployeeID,
                        EmpFirstName = request.EmpFirstName
                    };
                    string insertQuery = @"
                    INSERT INTO [tblNbNotification] (NotificationTitle, PathURL, status, createdon, createdby, EmployeeID, EmpFirstName)
                    VALUES (@NotificationTitle, @PathURL, @status, @createdon, @createdby, @EmployeeID, @EmpFirstName);
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
                        EmpFirstName = request.EmpFirstName,
                        NBNotificationID = request.NBNotificationID
                    };
                    string updateQuery = @"
                    UPDATE [tblNbNotification]
                    SET NotificationTitle = @NotificationTitle,
                    PathURL = @PathURL,
                    status = @status,
                    modifiedon = @modifiedon,
                    modifiedby = @modifiedby,
                    EmployeeID = @EmployeeID,
                    EmpFirstName = @EmpFirstName
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
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<Notification>>> GetAllNotificationsList()
        {
            try
            {
                string selectQuery = @"
            SELECT NBNotificationID, NotificationTitle, NotificationDetails, NotificationLink,
                   CourseID, ClassID, BoardId, ModuleName, Message, Platform
            FROM tblNbNotification";

                var data = await _connection.QueryAsync<Notification>(selectQuery);

                if (data != null)
                {
                    return new ServiceResponse<List<Notification>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Notification>>(false, "Records Not Found", new List<Notification>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Notification>>(false, ex.Message, new List<Notification>(), 500);
            }
        }
        public async Task<ServiceResponse<NotificationDTO>> GetNotificationById(int NotificationId)
        {
            try
            {
                NotificationDTO response = new();
                string selectQuery = @"
            SELECT * 
            FROM tblNbNotification
            WHERE NBNotificationID = @NotificationId";
                var data = await _connection.QuerySingleOrDefaultAsync<Notification>(selectQuery, new { NotificationId });
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

                    return new ServiceResponse<NotificationDTO>(true, "Records Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<NotificationDTO>(true, "Records Found", new NotificationDTO(), 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationDTO>(false, ex.Message, new NotificationDTO(), 500);
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
                NotificationLink = @NotificationLink,
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
                          WHERE [SOTDID] = @SOTDID;";
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
        private List<NbNotificationBoard> GetListOfNBBoards(int notificationId)
        {
            var boardquery = @" SELECT * FROM [tblNbNotificationBoard] WHERE NbNotificationBoardId = @NbNotificationBoardId;";

            // Execute the SQL query with the SOTDID parameter
            var boardData = _connection.Query<NbNotificationBoard>(boardquery, new { NbNotificationBoardId = notificationId });
            return boardData != null ? boardData.AsList() : [];
        }
        private List<NbNotificationCategory> GetListOfNBCategory(int notificationId)
        {
            var query = @"SELECT * FROM [tblNbNotificationCategory] WHERE  NBNotificationID = @NBNotificationID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NbNotificationCategory>(query, new { NBNotificationID = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationClass> GetListOfNBClass(int notificationId)
        {
            var query = @"SELECT * FROM [tblNbNotificationClass] WHERE  NBNotificationID = @NBNotificationID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NbNotificationClass>(query, new { NBNotificationID = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationCourse> GetListOfNBCourse(int notificationId)
        {
            var query = @"SELECT * FROM [tblNbNotificationCourse] WHERE  NBNotificationID = @NBNotificationID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NbNotificationCourse>(query, new { NBNotificationID = notificationId });
            return data != null ? data.AsList() : [];
        }
        private List<NbNotificationExamType> GetListOfNBExamType(int notificationId)
        {
            var query = @"SELECT * FROM [tblNbNotificationExamType] WHERE  NBNotificationID = @NBNotificationID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<NbNotificationExamType>(query, new { NBNotificationID = notificationId });
            return data != null ? data.AsList() : [];
        }
    }
}
