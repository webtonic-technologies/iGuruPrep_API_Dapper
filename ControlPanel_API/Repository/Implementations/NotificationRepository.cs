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
        public async Task<ServiceResponse<int>> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                if (request.NBNotificationID == 0)
                {
                    string imageUrl = string.Empty;

                    if (request.PathURL != null)
                    {
                        var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Notifications");
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                        }
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.PathURL.FileName);
                        var filePath = Path.Combine(uploads, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await request.PathURL.CopyToAsync(fileStream);
                        }
                        imageUrl = fileName;
                    }

                    var newNotification = new Notification
                    {
                        NBNotificationID = request.NBNotificationID,
                        BoardId = request.BoardId,
                        ClassID = request.ClassID,
                        CourseID = request.CourseID,
                        Message = request.Message,
                        ModuleName = request.ModuleName,
                        NotificationDetails = request.NotificationDetails,
                        NotificationLink = request.NotificationLink,
                        NotificationTitle = request.NotificationTitle,
                        PathURL = imageUrl,
                        Platform = request.Platform
                    };

                    string insertQuery = @"
            INSERT INTO tblNbNotification (NotificationTitle, NotificationDetails, NotificationLink, CourseID, PathURL, ClassID, BoardId, ModuleName, Message, Platform)
            VALUES (@NotificationTitle, @NotificationDetails, @NotificationLink, @CourseID, @PathURL, @ClassID, @BoardId, @ModuleName, @Message, @Platform);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                    int notificationId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, newNotification);
                    if (notificationId > 0)
                    {
                        var data = await AddUpdateNotificationDetailsMaster(request.NotificationDetailMasters, notificationId);
                        var data1 = await AddUpdateNotificationLinkMaster(request.NotificationLinkMasters, notificationId);
                        if (data1 > 0 && data > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation Successful", notificationId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Opertion Failed", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occured", 0, 500);
                    }
                }
                else
                {
                    string imageUrl = string.Empty;

                    if (request.PathURL != null)
                    {
                        var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Notifications");
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                        }
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.PathURL.FileName);
                        var filePath = Path.Combine(uploads, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await request.PathURL.CopyToAsync(fileStream);
                        }
                        imageUrl = fileName;
                    }

                    var newNotification = new Notification
                    {
                        NBNotificationID = request.NBNotificationID,
                        BoardId = request.BoardId,
                        ClassID = request.ClassID,
                        CourseID = request.CourseID,
                        Message = request.Message,
                        ModuleName = request.ModuleName,
                        NotificationDetails = request.NotificationDetails,
                        NotificationLink = request.NotificationLink,
                        NotificationTitle = request.NotificationTitle,
                        PathURL = imageUrl,
                        Platform = request.Platform
                    };
                    string updateQuery = @"
            UPDATE tblNbNotification
            SET NotificationTitle = @NotificationTitle,
                NotificationDetails = @NotificationDetails,
                NotificationLink = @NotificationLink,
                CourseID = @CourseID,
                PathURL = @PathURL,
                ClassID = @ClassID,
                BoardId = @BoardId,
                ModuleName = @ModuleName,
                Message = @Message,
                Platform = @Platform
            WHERE NBNotificationID = @NBNotificationID";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newNotification);
                    if (rowsAffected > 0)
                    {
                        var data = await AddUpdateNotificationDetailsMaster(request.NotificationDetailMasters, request.NBNotificationID);
                        var data1 = await AddUpdateNotificationLinkMaster(request.NotificationLinkMasters, request.NBNotificationID);
                        if (data1 > 0 && data > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation Successful", request.NBNotificationID, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Opertion Failed", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occured", 0, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
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
                NotificationDTO response = new NotificationDTO();
                string selectQuery = @"
            SELECT NBNotificationID, NotificationTitle, NotificationDetails, NotificationLink,
                   CourseID, ClassID, BoardId, ModuleName, Message, Platform
            FROM tblNbNotification
            WHERE NBNotificationID = @NotificationId";
                var data = await _connection.QuerySingleOrDefaultAsync<Notification>(selectQuery, new { NotificationId });
                if (data != null)
                {
                    string selectNLinkQuery = @"
            SELECT NL_id, NBNotificationID, NotificationTitle, NotificationLink
            FROM tblNbNotificationLinkMaster
            WHERE NBNotificationID = @NotificationId";
                    var links = await _connection.QueryAsync<NotificationLinkMaster>(selectNLinkQuery, new { NotificationId });
                    if (links != null)
                        response.NotificationLinkMasters = links.AsList();
                    string selectNDetailQuery = @"
            SELECT *
            FROM tblNbNotificationDetailMaster
            WHERE NBNotificationID = @NotificationId";
                    var details = await _connection.QueryAsync<NotificationDetail>(selectNDetailQuery, new { NotificationId });
                    if (details != null)
                        response.NotificationDetailMasters = details.AsList();

                    response.NBNotificationID = data.NBNotificationID;
                    response.NotificationTitle = data.NotificationTitle;
                    response.NotificationDetails = data.NotificationDetails;
                    response.NotificationLink = data.NotificationLink;
                    response.CourseName = GetCourseNameById(data.CourseID);
                    response.ClassName = GetClassNameById(data.ClassID);
                    response.BoardName = GetBoardNameById(data.BoardId);
                    response.ModuleName = data.ModuleName;
                    response.Message = data.Message;
                    response.Platform = data.Platform;

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

        public async Task<ServiceResponse<byte[]>> GetNotificationFileById(int NotificationId)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<Notification>(
                    "SELECT PathURL FROM tblNbNotification WHERE NBNotificationID = @NBNotificationID",
                    new { NBNotificationID = NotificationId });

                if (data == null)
                {
                    throw new Exception("Data not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Notifications", data.PathURL);

                if (!File.Exists(filePath))
                {
                    throw new Exception("File not found");
                }
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                return new ServiceResponse<byte[]>(true, "Record Found", fileBytes, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, Array.Empty<byte>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateNotificationFile(NotificationImageDTO request)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<Notification>(
                    "SELECT PathURL FROM tblNbNotification WHERE NBNotificationID = @NBNotificationID",
                    new { request.NBNotificationID });

                if (data == null)
                {
                    throw new Exception("data not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Notifications", data.PathURL);
                if (File.Exists(filePath) && !string.IsNullOrWhiteSpace(data.PathURL))
                {
                    File.Delete(filePath);
                }

                if (request.PathURL != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Notifications");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.PathURL.FileName);
                    var newFilePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await request.PathURL.CopyToAsync(fileStream);
                    }
                    data.PathURL = fileName;
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "UPDATE tblNbNotification SET PathURL = @PathURL WHERE NBNotificationID = @NBNotificationID",
                    new { data.PathURL, request.NBNotificationID });
                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Notification Updated Successfully", 200);
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
        private async Task<int> AddUpdateNotificationDetailsMaster (List<NotificationDetail>? request, int notificationId)
        {
            int rowsAffected = 0;
            string insertQuery = @"
            INSERT INTO tblNbNotificationDetailMaster (NBNotificationID, NotificationDetails)
            VALUES (@NBNotificationID, @NotificationDetails)";

            string updateQuery = @"
            UPDATE tblNbNotificationDetailMaster
            SET NBNotificationID = @NBNotificationID,
                NotificationDetails = @NotificationDetails
            WHERE ND_id = @ND_id";
            if(request != null)
            {
                foreach (var data in request)
                {
                    var newNDetails = new NotificationDetail
                    {
                        NBNotificationID = notificationId,
                        NotificationDetails = data.NotificationDetails
                    };
                    if (data.ND_id == 0)
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
        private async Task<int> AddUpdateNotificationLinkMaster (List<NotificationLinkMaster>? request, int notificationId)
        {
            int rowsAffected = 0;

            string insertQuery = @"
            INSERT INTO tblNbNotificationLinkMaster (NBNotificationID, NotificationTitle, NotificationLink)
            VALUES (@NBNotificationID, @NotificationTitle, @NotificationLink)";

            string updateQuery = @"
            UPDATE tblNbNotificationLinkMaster
            SET NBNotificationID = @NBNotificationID,
                NotificationTitle = @NotificationTitle,
                NotificationLink = @NotificationLink
            WHERE NL_id = @NL_id";
            if(request != null)
            {
                foreach (var data in request)
                {
                    var newNLink = new NotificationLinkMaster
                    {
                        NBNotificationID = notificationId,
                        NotificationLink = data.NotificationLink,
                        NotificationTitle = data.NotificationTitle,
                    };
                    if (data.NL_id == 0)
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

        private string GetBoardNameById(int? boardId)
        {
            string sql = "SELECT BoardName FROM tblBoard WHERE BoardId = @BoardId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { BoardId = boardId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        // Helper method to get class name by ID
        private string GetClassNameById(int? classId)
        {
            string sql = "SELECT ClassName FROM tblClass WHERE ClassId = @ClassId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { ClassId = classId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }

        // Helper method to get course name by ID
        private string GetCourseNameById(int? courseId)
        {
            string sql = "SELECT CourseName FROM tblCourse WHERE CourseId = @CourseId";
            var data = _connection.QueryFirstOrDefault<string>(sql, new { CourseId = courseId });
            if (data != null)
            {
                return data;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
