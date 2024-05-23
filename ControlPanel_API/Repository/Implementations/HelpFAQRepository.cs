using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class HelpFAQRepository : IHelpFAQRepository
    {
        private readonly IDbConnection _connection;

        public HelpFAQRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateFAQ(HelpFAQ request)
        {
            try
            {
                if (request.HelpFAQId == 0)
                {
                    string insertQuery = @" INSERT INTO [tblHelpFAQ] (
                    FAQName, FAQAnswer, Status, CreatedOn, CreatedBy, EmployeeID, EmpFirstName) 
                    VALUES (@FAQName, @FAQAnswer, @Status, @CreatedOn, @CreatedBy, @EmployeeID, @EmpFirstName);";
                    int insertedValue = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.FAQName,
                        request.FAQAnswer,
                        Status = true,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.EmpFirstName
                    });
                    if (insertedValue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "FAQ Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateQuery = @" UPDATE tblHelpFAQ SET FAQName = @FAQName, FAQAnswer = @FAQAnswer, Status = @Status,
                                            ModifiedOn = @ModifiedOn, ModifiedBy = @ModifiedBy, EmployeeID = @EmployeeID,
                                            EmpFirstName = @EmpFirstName WHERE HelpFAQId = @HelpFAQId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.FAQName,
                        request.FAQAnswer,
                        request.Status,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.HelpFAQId
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "FAQ Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status404NotFound);
                    }

                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<HelpFAQ>> GetFAQById(int faqId)
        {
            try
            {
                var sql = "SELECT * FROM [tblHelpFAQ] WHERE [HelpFAQId] = @HelpFAQId;";
                var data = await _connection.QueryFirstOrDefaultAsync<HelpFAQ>(sql, new { HelpFAQId = faqId });
                if (data != null)
                {
                    return new ServiceResponse<HelpFAQ>(true, "Records Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<HelpFAQ>(false, "Records Not Found", new HelpFAQ(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<HelpFAQ>(false, ex.Message, new HelpFAQ(), 500);
            }
        }

        public async Task<ServiceResponse<List<HelpFAQ>>> GetFAQList()
        {
            try
            {
                var sql = "SELECT * FROM tblHelpFAQ;";
                var data = await _connection.QueryAsync<HelpFAQ>(sql);

                if (data.Any())
                {
                    return new ServiceResponse<List<HelpFAQ>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<HelpFAQ>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<HelpFAQ>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetFAQById(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status;

                    string sql = "UPDATE [tblHelpFAQ] SET Status = @Status WHERE [HelpFAQId] = @HelpFAQId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { data.Data.Status, HelpFAQId = id });
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
