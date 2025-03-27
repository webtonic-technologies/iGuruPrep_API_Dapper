using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
using Packages_API.Repository.Interfaces;
using Packages_API.Services.Interfaces;

namespace Packages_API.Services.Implementations
{
    public class CoinsServices : ICoinsServices
    {
        private readonly ICoinsRepository _coinsRepository;

        public CoinsServices(ICoinsRepository coinsRepository)
        {
            _coinsRepository = coinsRepository;
        }
        public async Task<ServiceResponse<bool>> AddUpdateCoins(Coins coin)
        {
            return await _coinsRepository.AddUpdateCoins(coin);
        }

        public async Task<ServiceResponse<bool>> CoinStatus(int coinId)
        {
            return await _coinsRepository.CoinStatus(coinId);
        }

        public async Task<ServiceResponse<bool>> DeleteCoin(int coinId)
        {
            return await _coinsRepository.DeleteCoin(coinId);
        }

        public async Task<ServiceResponse<List<Coins>>> GetAllCoins()
        {
            return await _coinsRepository.GetAllCoins();
        }

        public async Task<ServiceResponse<Coins>> GetCoin(int coinId)
        {
            return await _coinsRepository.GetCoin(coinId);
        }
    }
}
