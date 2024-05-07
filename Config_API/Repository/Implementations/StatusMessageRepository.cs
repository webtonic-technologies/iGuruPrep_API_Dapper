using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
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
                    string query = @"INSERT INTO [tblStatusMessage] (StatusCode, StatusMessage, createdon, createdby, EmployeeID, EmpFirstName)
                             VALUES (@StatusCode, @StatusMessage, GETDATE(), @CreatedBy, @EmployeeID, @EmpFirstName)";

                    int rowsAffected = await _connection.ExecuteAsync(query, new
                    {
                        request.StatusCode,
                        request.StatusMessage,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.EmpFirstName,
                    });
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
                    string query = @"UPDATE [tblStatusMessage]
                             SET StatusCode = @StatusCode, 
                                 StatusMessage = @StatusMessage,
                                 modifiedon = GETDATE(),
                                 modifiedby = @ModifiedBy
                                EmpFirstName = @EmpFirstName
                             WHERE StatusId = @StatusId";
                    int rowsAffected = await _connection.ExecuteAsync(query, new
                    {
                        request.StatusCode,
                        request.StatusMessage,
                        request.StatusId,
                        request.modifiedby,
                        request.EmpFirstName,
                        modifiedon = DateTime.Now
                    });
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
                string query = @"SELECT StatusId, StatusCode, StatusMessage, modifiedon, modifiedby, createdon, createdby, EmployeeID, EmpFirstName 
                             FROM [tblStatusMessage]
                             WHERE StatusId = @StatusId";

                var data = await _connection.QueryFirstOrDefaultAsync<StatusMessages>(query, new { StatusId = id });

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
                string query = @"SELECT StatusId, StatusCode, StatusMessage, modifiedon, modifiedby, createdon, createdby, EmployeeID, EmpFirstName 
                             FROM [tblStatusMessage]";

                var data = await _connection.QueryAsync<StatusMessages>(query);

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
