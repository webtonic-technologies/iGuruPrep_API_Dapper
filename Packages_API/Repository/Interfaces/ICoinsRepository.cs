using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
namespace Packages_API.Repository.Interfaces
{
    public interface ICoinsRepository
    {
        Task<ServiceResponse<bool>> AddUpdateCoins(Coins coin);
        Task<ServiceResponse<List<Coins>>> GetAllCoins();
        Task<ServiceResponse<Coins>> GetCoin(int coinId);
        Task<ServiceResponse<bool>> DeleteCoin(int coinId);
        Task<ServiceResponse<bool>> CoinStatus(int coinId);
    }
}