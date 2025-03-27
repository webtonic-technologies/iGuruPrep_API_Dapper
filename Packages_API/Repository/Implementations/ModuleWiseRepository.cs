using System.Data;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
using Packages_API.DTOs.Response;
using Dapper;
using Packages_API.Repository.Interfaces;

namespace Packages_API.Repository.Implementations
{
    public class ModuleWiseRepository : IModuleWiseRepository
    {

        private readonly IDbConnection _connection;

        public ModuleWiseRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<ModuleDTO>>> GetModules()
        {
            try
            {
                string query = "SELECT ModuleID, ModuleName FROM tblModule where ParentModuleID = 0";
                var modules = (await _connection.QueryAsync<ModuleDTO>(query)).ToList();

                if (modules.Count == 0)
                    return new ServiceResponse<List<ModuleDTO>>(false, "No modules found.", [], 404);

                return new ServiceResponse<List<ModuleDTO>>(true, "Modules fetched successfully.", modules, 200, modules.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ModuleDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<bool>> SetModuleWiseConfiguration(List<ModuleWiseConfigDTO> configs)
        {
            if (configs == null || !configs.Any())
                return new ServiceResponse<bool>(false, "No module configurations provided.", false, 400);

            try
            {
                // Open connection explicitly
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Delete existing configurations for provided ModuleIDs
                        var moduleIds = configs.Select(c => c.ModuleID).Distinct().ToList();
                        string deleteQuery = "DELETE FROM tblModulewiseConfiguration WHERE ModuleID IN @ModuleIDs";
                        await _connection.ExecuteAsync(deleteQuery, new { ModuleIDs = moduleIds }, transaction);

                        // Step 2: Insert new configurations in batch
                        string insertQuery = @"INSERT INTO tblModulewiseConfiguration (ModuleID, IsFree, IsSubscription, DiscountOnFinalPrice) 
                                       VALUES (@ModuleID, @IsFree, @IsSubscription, @DiscountOnFinalPrice)";
                        await _connection.ExecuteAsync(insertQuery, configs, transaction);

                        // Commit transaction
                        transaction.Commit();

                        return new ServiceResponse<bool>(true, "Module-wise configurations updated successfully.", true, 200);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new ServiceResponse<bool>(false, ex.Message, false, 500);
                    }
                    finally
                    {
                        // Close connection explicitly
                        if (_connection.State == ConnectionState.Open)
                            _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<List<ModuleWiseConfigDTO>>> GetModuleWiseConfiguration()
        {
            try
            {
                string query = "SELECT * FROM tblModulewiseConfiguration";
                var configList = (await _connection.QueryAsync<ModuleWiseConfigDTO>(query)).ToList();

                if (configList == null || !configList.Any())
                    return new ServiceResponse<List<ModuleWiseConfigDTO>>(false, "No configurations found.", null, 404);

                return new ServiceResponse<List<ModuleWiseConfigDTO>>(true, "Module configurations fetched successfully.", configList, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ModuleWiseConfigDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}