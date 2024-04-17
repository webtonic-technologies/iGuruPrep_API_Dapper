using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class StatusMessageRepository : IStatusMessageRepository
    {
        private readonly IDbConnection _connection;

        public StatusMessageRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateStatusMessage(StatusMessages request)
        {

            try
            {
                if (request.StatusId == 0)
                {
                    // Insert new status message
                    var sql = "INSERT INTO tblStatusMessage (StatusCode, StatusMessage) VALUES (@StatusCode, @StatusMessage)";
                    int rowsAffected = await _connection.ExecuteAsync(sql, new { request.StatusCode, request.StatusMessage });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Status Message Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    // Update existing status message
                    var sql = "UPDATE tblStatusMessage SET StatusCode = @StatusCode, StatusMessage = @StatusMessage WHERE StatusId = @StatusId";
                    int rowsAffected = await _connection.ExecuteAsync(sql, new { request.StatusCode, request.StatusMessage, request.StatusId });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Status Message Updated Successfully", 200);
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

        public async Task<ServiceResponse<StatusMessages>> GetStatusMessageById(int id)
        {
            try
            {
                string sql = "SELECT * FROM tblStatusMessage WHERE StatusId = @StatusId";

                var data = await _connection.QueryFirstOrDefaultAsync<StatusMessages>(sql, new { StatusId = id });

                if (data != null)
                {
                    return new ServiceResponse<StatusMessages>(true, "Record Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<StatusMessages>(false, "Record not Found", new StatusMessages(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StatusMessages>(false, ex.Message, new StatusMessages(), 500);
            }
        }
        public async Task<ServiceResponse<List<StatusMessages>>> GetStatusMessageList()
        {
            try
            {
                string sql = "SELECT * FROM tblStatusMessage";

                var data = await _connection.QueryAsync<StatusMessages>(sql);

                if (data != null)
                {
                    return new ServiceResponse<List<StatusMessages>>(true, "Record Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<StatusMessages>>(false, "Record not Found", [], 500);
                }
            }
            catch(Exception ex)
            {
                return new ServiceResponse<List<StatusMessages>>(false, ex.Message, [], 500);
            }
        }
    }
}
