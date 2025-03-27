using System.Data;
using Dapper;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
using Packages_API.Repository.Interfaces;

namespace Packages_API.Repository.Implementations
{
    public class CoinsRepository : ICoinsRepository
    {
        private readonly IDbConnection _connection;

        public CoinsRepository(IDbConnection connection)
        {
            _connection = connection;
        }


        // ✅ Add or Update Coin
        public async Task<ServiceResponse<bool>> AddUpdateCoins(Coins coin)
        {
            try
            {
                string query = @"
                IF EXISTS (SELECT 1 FROM tblCoins WHERE CoinID = @CoinID)
                BEGIN
                    UPDATE tblCoins
                    SET NoOfCoins = @NoOfCoins, Price = @Price, IsActive = @IsActive
                    WHERE CoinID = @CoinID;
                END
                ELSE
                BEGIN
                    INSERT INTO tblCoins (NoOfCoins, Price, IsActive, IsDeleted)
                    VALUES (@NoOfCoins, @Price, 1, 0);
                END";

                int rowsAffected = await _connection.ExecuteAsync(query, coin);
                return new ServiceResponse<bool>(rowsAffected > 0, "Coin added/updated successfully", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        // ✅ Get All Coins
        public async Task<ServiceResponse<List<Coins>>> GetAllCoins()
        {
            try
            {
                string query = "SELECT * FROM tblCoins WHERE IsDeleted = 0";
                var coins = (await _connection.QueryAsync<Coins>(query)).ToList();

                if (!coins.Any())
                    return new ServiceResponse<List<Coins>>(false, "No coins found.", new List<Coins>(), 200);

                return new ServiceResponse<List<Coins>>(true, "Coins fetched successfully", coins, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Coins>>(false, ex.Message, null, 500);
            }
        }

        // ✅ Get Coin by ID
        public async Task<ServiceResponse<Coins>> GetCoin(int coinId)
        {
            try
            {
                string query = "SELECT * FROM tblCoins WHERE CoinID = @CoinID AND IsDeleted = 0";
                var coin = await _connection.QueryFirstOrDefaultAsync<Coins>(query, new { CoinID = coinId });

                if (coin == null)
                    return new ServiceResponse<Coins>(false, "Coin not found.", null, 404);

                return new ServiceResponse<Coins>(true, "Coin fetched successfully", coin, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Coins>(false, ex.Message, null, 500);
            }
        }

        // ✅ Delete Coin (Soft Delete)
        public async Task<ServiceResponse<bool>> DeleteCoin(int coinId)
        {
            try
            {
                string query = "UPDATE tblCoins SET IsDeleted = 1 WHERE CoinID = @CoinID";
                int rowsAffected = await _connection.ExecuteAsync(query, new { CoinID = coinId });

                if (rowsAffected == 0)
                    return new ServiceResponse<bool>(false, "Coin not found or already deleted.", false, 404);

                return new ServiceResponse<bool>(true, "Coin deleted successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        // ✅ Change Coin Status (Active/Inactive)
        public async Task<ServiceResponse<bool>> CoinStatus(int coinId)
        {
            try
            {
                // Step 1: Fetch the existing status
                string getStatusQuery = "SELECT IsActive FROM tblCoins WHERE CoinID = @CoinID AND IsDeleted = 0";
                bool? currentStatus = await _connection.QueryFirstOrDefaultAsync<bool?>(getStatusQuery, new { CoinID = coinId });

                // If no record found
                if (currentStatus == null)
                    return new ServiceResponse<bool>(false, "Coin not found or deleted.", false, 404);

                // Step 2: Toggle the status
                bool newStatus = !currentStatus.Value;

                // Step 3: Update the status
                string updateQuery = "UPDATE tblCoins SET IsActive = @IsActive WHERE CoinID = @CoinID";
                int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { CoinID = coinId, IsActive = newStatus });

                // Step 4: Return the response
                if (rowsAffected == 0)
                    return new ServiceResponse<bool>(false, "Failed to update coin status.", false, 500);

                return new ServiceResponse<bool>(true, $"Coin status updated to {(newStatus ? "Active" : "Inactive")}.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

    }
}