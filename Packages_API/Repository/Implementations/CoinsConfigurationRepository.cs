using System.Data.Common;
using System.Data;
using Packages_API.DTOs.ServiceResponse;
using Dapper;
using Packages_API.DTOs.Requests;
using Packages_API.Repository.Interfaces;

namespace Packages_API.Repository.Implementations
{
   
    public class CoinsConfigurationRepository: ICoinsConfigurationRepository
    {
        private readonly IDbConnection _connection;

        public CoinsConfigurationRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<bool>> AddUpdateCoinConfiguration(AddUpdateCoinConfigurationRequest request)
        {
            try
            {
                string query;

                // Fetch the CoinCategoryType based on CoinCategoryID
                string categoryQuery = "SELECT CoinCategoryType FROM tblCoinCategory WHERE CoinCategoryID = @CoinCategoryID";
                string coinCategoryType = await _connection.ExecuteScalarAsync<string>(categoryQuery, new { request.CoinCategoryID });

                if (coinCategoryType == null)
                {
                    return new ServiceResponse<bool>(false, "Invalid CoinCategoryID.", false, 400);
                }

                if (request.CCID == 0)
                {
                    // Insert Query based on Coin Category
                    if (coinCategoryType == "Basic")
                    {
                        query = @"INSERT INTO tblCoinConfiguration 
                          (CoinCategoryID, Name, Rupees, NoOfCoins, IsActive) 
                          VALUES (@CoinCategoryID, @Name, @Rupees, @NoOfCoins, 1)";
                    }
                    else if (coinCategoryType == "LeaderBoard")
                    {
                        query = @"INSERT INTO tblCoinConfiguration 
                          (CoinCategoryID, Name, NoOfCoins, IsActive) 
                          VALUES (@CoinCategoryID, @Name, @NoOfCoins, 1)";
                    }
                    else if (coinCategoryType == "Badges")
                    {
                        query = @"INSERT INTO tblCoinConfiguration 
                          (CoinCategoryID, Name, StartRange, EndRange, IsActive) 
                          VALUES (@CoinCategoryID, @Name, @StartRange, @EndRange, 1)";
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Invalid Coin Category Type.", false, 400);
                    }
                }
                else
                {
                    // Update Query based on Coin Category
                    if (coinCategoryType == "Basic")
                    {
                        query = @"UPDATE tblCoinConfiguration 
                          SET CoinCategoryID = @CoinCategoryID, Name = @Name, Rupees = @Rupees, 
                              NoOfCoins = @NoOfCoins
                          WHERE CCID = @CCID";
                    }
                    else if (coinCategoryType == "LeaderBoard")
                    {
                        query = @"UPDATE tblCoinConfiguration 
                          SET CoinCategoryID = @CoinCategoryID, Name = @Name, NoOfCoins = @NoOfCoins
                          WHERE CCID = @CCID";
                    }
                    else if (coinCategoryType == "Badges")
                    {
                        query = @"UPDATE tblCoinConfiguration 
                          SET CoinCategoryID = @CoinCategoryID, Name = @Name, StartRange = @StartRange, 
                              EndRange = @EndRange
                          WHERE CCID = @CCID";
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Invalid Coin Category Type.", false, 400);
                    }
                }

                await _connection.ExecuteAsync(query, request);
                return new ServiceResponse<bool>(true, "Coin Configuration saved successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<List<CoinConfigurationDTO>>> GetAllCoinConfigurations()
        {
            try
            {
                string query = @"
                SELECT CCID, CoinCategoryID, Name,
                    CASE WHEN CoinCategoryID = 1 THEN Rupees ELSE NULL END AS Rupees,
                    CASE WHEN CoinCategoryID IN (1,2) THEN NoOfCoins ELSE NULL END AS NoOfCoins,
                    CASE WHEN CoinCategoryID = 3 THEN StartRange ELSE NULL END AS StartRange,
                    CASE WHEN CoinCategoryID = 3 THEN EndRange ELSE NULL END AS EndRange,
                    IsActive
                FROM tblCoinConfiguration";

                var configurations = (await _connection.QueryAsync<CoinConfigurationDTO>(query)).ToList();
                return new ServiceResponse<List<CoinConfigurationDTO>>(true, "Fetched successfully.", configurations, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CoinConfigurationDTO>>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<CoinConfigurationDTO>> GetCoinConfigurationByID(int ccid)
        {
            try
            {
                string query = @"
                SELECT CCID, CoinCategoryID, Name,
                    CASE WHEN CoinCategoryID = 1 THEN Rupees ELSE NULL END AS Rupees,
                    CASE WHEN CoinCategoryID IN (1,2) THEN NoOfCoins ELSE NULL END AS NoOfCoins,
                    CASE WHEN CoinCategoryID = 3 THEN StartRange ELSE NULL END AS StartRange,
                    CASE WHEN CoinCategoryID = 3 THEN EndRange ELSE NULL END AS EndRange,
                    IsActive
                FROM tblCoinConfiguration
                WHERE CCID = @CCID";

                var configuration = await _connection.QueryFirstOrDefaultAsync<CoinConfigurationDTO>(query, new { CCID = ccid });

                if (configuration == null)
                    return new ServiceResponse<CoinConfigurationDTO>(false, "Coin Configuration not found.", null, 404);

                return new ServiceResponse<CoinConfigurationDTO>(true, "Fetched successfully.", configuration, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CoinConfigurationDTO>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<bool>> CoinConfigurationStatus(int ccid)
        {
            try
            {
                string query = "UPDATE tblCoinConfiguration SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE CCID = @CCID";
                await _connection.ExecuteAsync(query, new { CCID = ccid });

                return new ServiceResponse<bool>(true, "Status toggled successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}