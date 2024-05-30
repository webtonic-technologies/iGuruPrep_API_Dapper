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
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                if (request.NotificationTemplateID == 0)
                {
                    string insertQuery = @"
                    INSERT INTO [tblNotificationTemplate] (NotificationTemplateID, Status, CreatedOn, CreatedBy, EmployeeID, EmpFirstName, SubModuleId)
                    OUTPUT Inserted.NotificationTemplateID
                    VALUES (@NotificationTemplateID, @Status, @CreatedOn, @CreatedBy, @EmployeeID, @EmpFirstName, @SubModuleId);";

                    using var transaction = _connection.BeginTransaction();
                    int insertedValue = await _connection.QuerySingleAsync<int>(insertQuery, new
                    {
                        Status = true,
                        request.moduleID,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.subModuleId
                    }, transaction);
                    transaction.Commit();

                    if (insertedValue > 0)
                    {
                        int mapping = NotificationTemplateMapping(request.NotificationTemplateMappings ??= ([]), insertedValue);
                        if (mapping > 0)
                        {
                            return new ServiceResponse<string>(true, "Opertion Successful", "Notification Template Added Successfully", 200);
                        }
                        else
                        {
                            transaction.Rollback();
                            return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                    }
                }
                else
                {
                    using var transaction = _connection.BeginTransaction();
                    string updateQuery = @"
            UPDATE [tblNotificationTemplate]
            SET 
                Status = @Status,
                ModuleID = @ModuleID,
                ModifiedOn = @ModifiedOn,
                ModifiedBy = @ModifiedBy,
                EmployeeID = @EmployeeID,
                EmpFirstName = @EmpFirstName,
                SubModuleId = @SubModuleId
            WHERE 
                NotificationTemplateID = @NotificationTemplateID;";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.Status,
                        request.moduleID,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.subModuleId,
                        request.NotificationTemplateID
                    }, transaction);
                    transaction.Commit();
                    if (rowsAffected > 0)
                    {
                        int mapping = NotificationTemplateMapping(request.NotificationTemplateMappings ??= ([]), request.NotificationTemplateID);
                        if (mapping > 0)
                        {
                            return new ServiceResponse<string>(true, "Opertion Successful", "Notification Template Updated Successfully", 200);
                        }
                        else
                        {
                            transaction.Rollback();
                            return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                        }
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
        public async Task<ServiceResponse<List<NotificationModule>>> GetAllModuleList()
        {
            try
            {
                string sql = "SELECT * FROM tblModule_new";
                var modules = await _connection.QueryAsync<NotificationModule>(sql);
                if (modules.Any())
                {
                    return new ServiceResponse<List<NotificationModule>>(true, "Records Found", modules.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<NotificationModule>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationModule>>(false, ex.Message, [], 500);
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
        public async Task<ServiceResponse<NotificationResponseDTO>> GetNotificationsById(int id)
        {
            try
            {
                NotificationResponseDTO response = new();
                var notificationQuery = @"Select * from tblNotificationTemplate where [NotificationTemplateID] = @id;";
                var data = _connection.QueryFirstOrDefault<NotificationTemplate>(notificationQuery, new { id });
                if (data != null)
                {
                    response.NotificationTemplateID = data.NotificationTemplateID;
                    response.Status = data.Status;
                    response.createdby = data.createdby;
                    response.createdon = data.createdon;
                    response.moduleID = data.moduleID;
                    response.modifiedon = data.modifiedon;
                    response.modifiedby = data.modifiedby;
                    response.EmployeeID = data.EmployeeID;
                    response.EmpFirstName = data.EmpFirstName;
                    response.subModuleId = data.subModuleId;

                    var moduleQuery = @"Select * from tblModule where ModuleID = @moduleId;";
                    var moduledata = _connection.QueryFirstOrDefault<NotificationModule>(moduleQuery, new { moduleId = data.moduleID });
                    response.ModuleName = moduledata != null ? moduledata.ModuleName : string.Empty;

                    var submoduleQuery = @"Select * from tblModule where ModuleID = @moduleId;";
                    var submoduledata = _connection.QueryFirstOrDefault<NotificationModule>(submoduleQuery, new { moduleId = data.subModuleId });
                    response.SubModuleName = submoduledata != null ? submoduledata.ModuleName : string.Empty;

                    response.NotificationTemplateMappings = await GetListOfNotificationMappings(data.NotificationTemplateID);

                    return new ServiceResponse<NotificationResponseDTO>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<NotificationResponseDTO>(false, "Record not found", new NotificationResponseDTO(), 500);
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
                var data = await _connection.QueryFirstOrDefaultAsync<NotificationTemplate>("SELECT * FROM tblNotificationTemplate WHERE NotificationTemplateID = @Id", new { Id = id });

                if (data != null)
                {
                    if (data.Status == 0)
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
        private int NotificationTemplateMapping(List<NotificationTemplateMapping> request, int NotificationTemplateID)
        {
            foreach (var data in request)
            {
                data.NotificationTemplateID = NotificationTemplateID;
            }
            string query = "SELECT COUNT(*) FROM [tblNotificationTemplateMapping] WHERE [NotificationTemplateID] = @NotificationTemplateID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NotificationTemplateID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNotificationTemplateMapping]
                          WHERE [NotificationTemplateID] = @NotificationTemplateID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { NotificationTemplateID });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"INSERT INTO [tblNotificationTemplateMapping] (NotificationTemplateID, PlatformID, [Message], isStudent, isEmployee)
                                         VALUES (@NotificationTemplateID, @PlatformID, @Message, @IsStudent, @IsEmployee);";

                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"INSERT INTO [tblNotificationTemplateMapping] (NotificationTemplateID, PlatformID, [Message], isStudent, isEmployee)
                                         VALUES (@NotificationTemplateID, @PlatformID, @Message, @IsStudent, @IsEmployee);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private async Task<List<NotificationTemplateMappingResponse>> GetListOfNotificationMappings(int NotificationTemplateID)
        {
            var response = new List<NotificationTemplateMappingResponse>();
            var query = @"SELECT * FROM [tblNotificationTemplateMapping] WHERE [NotificationTemplateID] = @NotificationTemplateID;";
            var data = _connection.Query<NotificationTemplateMapping>(query, new { NotificationTemplateID });
            foreach (var item in data)
            {
                List<int> ids = item.PlatformID
       .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
       .Select(id => int.Parse(id.Trim()))
       .ToList();
                var platformQuery = @"select * from tblPlatform WHERE platformid IN @ids;";
                var platforms = await _connection.QueryAsync<Platform>(platformQuery, ids);
                var mapping = new NotificationTemplateMappingResponse
                {
                    TemplateMappingId = item.TemplateMappingId,
                    NotificationTemplateID = item.NotificationTemplateID,
                    PlatformDatas = platforms,
                    Role = item.isEmployee == true ? "Employee" : item.isStudent == true ? "Student" : string.Empty,
                    Message = item.Message,
                    isStudent = item.isStudent,
                    isEmployee = item.isEmployee
                };
                response.Add(mapping);
            }
            return response != null ? response.AsList() : [];
        }
        public async Task<ServiceResponse<List<NotificationResponseDTO>>> GetListofNotifications()
        {
            try
            {
                List<NotificationResponseDTO> response = [];
                var notificationQuery = @"Select * from tblNotificationTemplate;";
                var data = await _connection.QueryAsync<NotificationTemplate>(notificationQuery);
                if (data.Any())
                {
                    response = data.Select(item => new NotificationResponseDTO
                    {
                        NotificationTemplateID = item.NotificationTemplateID,
                        Status = item.Status,
                        createdby = item.createdby,
                        createdon = item.createdon,
                        moduleID = item.moduleID,
                        modifiedon = item.modifiedon,
                        modifiedby = item.modifiedby,
                        EmployeeID = item.EmployeeID,
                        EmpFirstName = item.EmpFirstName,
                        subModuleId = item.subModuleId
                    }).ToList();

                    foreach (var record in response)
                    {
                        record.NotificationTemplateMappings = await GetListOfNotificationMappings(record.NotificationTemplateID);

                        var moduleQuery = @"Select * from tblModule where ModuleID = @moduleId;";
                        var moduledata = _connection.QueryFirstOrDefault<NotificationModule>(moduleQuery, new { moduleId = record.moduleID });
                        record.ModuleName = moduledata != null ? moduledata.ModuleName : string.Empty;

                        var submoduleQuery = @"Select * from tblModule where ModuleID = @moduleId;";
                        var submoduledata = _connection.QueryFirstOrDefault<NotificationModule>(submoduleQuery, new { moduleId = record.subModuleId });
                        record.SubModuleName = submoduledata != null ? submoduledata.ModuleName : string.Empty;
                    }
                    return new ServiceResponse<List<NotificationResponseDTO>>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<List<NotificationResponseDTO>>(false, "Record not found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationResponseDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}