using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class NotificationModRepository : INotificationModRepository
    {
        private readonly IDbConnection _connection;
        public NotificationModRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationTemplate request)
        {
            try
            {
                if (request.NotificationTemplateID == 0)
                {
                    string insertSql = @"INSERT INTO tblNotificationTemplate (Notification, Status, CreatedBy, CreatedDate, platformid, moduleid)
                                     VALUES (@Notification, @Status, @CreatedBy, @CreatedDate, @PlatformId, @ModuleId)";
                    int rowsAffected = await _connection.ExecuteAsync(insertSql, request);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Notification Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                    }
                }
                else
                {
                    string updateSql = @"UPDATE tblNotificationTemplate
                                     SET Notification = @Notification,
                                         Status = @Status,
                                         CreatedBy = @CreatedBy,
                                         CreatedDate = @CreatedDate,
                                         platformid = @PlatformId,
                                         moduleid = @ModuleId
                                     WHERE NotificationTemplateID = @NotificationTemplateID";
                    int rowsAffected = await _connection.ExecuteAsync(updateSql, request);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Notification Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<Module>>> GetAllModuleList()
        {
            try
            {
                string sql = "SELECT * FROM tblModule_new";
                var modules = await _connection.QueryAsync<Module>(sql);
                if (modules.Any())
                {
                    return new ServiceResponse<List<Module>>(true, "Records Found", modules.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Module>>(false, "Records Not Found", new List<Module>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Module>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<Platform>>> GetAllPlatformList()
        {
            try
            {
                string sql = "SELECT * FROM tblPlatform";
                var data = await _connection.QueryAsync<Platform>(sql);
                if (data.Any())
                {
                    return new ServiceResponse<List<Platform>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Platform>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Platform>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<NotificationModuleDTO>> GetNotificationsByModuleId(int id)
        {
            try
            {
                var response = new NotificationModuleDTO();
                var moduleData = await _connection.QuerySingleOrDefaultAsync<Module>("SELECT * FROM tblModule_new WHERE @ModuleID = ModuleID", new { ModuleID = id });
                if (moduleData != null)
                {
                    response.ModuleName = moduleData.ModuleName;
                }
                string sql = @"SELECT [NotificationTemplateID],
                                      [PlatformID],
                                      [Message],
                                      [Status],
                                      [moduleID],
                                      [modifiedon],
                                      [modifiedby],
                                      [createdon],
                                      [createdby],
                                      [EmployeeID] FROM tblNotificationTemplate WHERE moduleid = @ModuleId";
                var templates = await _connection.QueryAsync<NotificationTemplate>(sql, new { ModuleId = id });
                if (templates != null)
                {
                    foreach (var data in templates)
                    {
                        string sqlQuery = "SELECT * FROM tblPlatform WHERE platformid = @PlatformId";
                        var platform = await _connection.QueryFirstOrDefaultAsync<Platform>(sqlQuery, new { PlatformId = data.PlatformID });

                        var item = new NotificationDTO
                        {
                            Platformname = platform != null ? platform.Platformname : string.Empty,
                            Message = data.Message,
                            NotificationTemplateID = data.NotificationTemplateID
                        };
                        var data1 = response.NotificationDTOs = [];
                            data1.Add(item);
                    }
                    return new ServiceResponse<NotificationModuleDTO>(true, "Records found", response, 500);
                }
                else
                {
                    return new ServiceResponse<NotificationModuleDTO>(false, "Records not found", response, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationModuleDTO>(false, ex.Message, new NotificationModuleDTO(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<NotificationTemplate>("SELECT * FROM tblNotificationTemplate WHERE NotificationTemplateID = @Id", new { Id = id });

                if (data != null)
                {
                    if(data.Status == 0)
                    {
                        data.Status = 1;
                    }
                    else
                    {
                        data.Status = 0;
                    }
                    string updateQuery = @"UPDATE tblNotificationTemplate SET Status = @Status WHERE NotificationTemplateID = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { data.Status, Id = id });

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
    }
}