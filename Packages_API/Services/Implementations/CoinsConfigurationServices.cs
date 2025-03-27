using Packages_API.DTOs.Requests;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Repository.Interfaces;
using Packages_API.Services.Interfaces;

namespace Packages_API.Services.Implementations
{
    public class CoinsConfigurationServices : ICoinsConfigurationServices
    {
        private readonly ICoinsConfigurationRepository _coinsConfigurationRepository;
        public CoinsConfigurationServices(ICoinsConfigurationRepository coinsConfigurationRepository)
        {
            _coinsConfigurationRepository = coinsConfigurationRepository;
        }
        public async Task<ServiceResponse<bool>> AddUpdateCoinConfiguration(AddUpdateCoinConfigurationRequest request)
        {
            return await _coinsConfigurationRepository.AddUpdateCoinConfiguration(request);
        }

        public async Task<ServiceResponse<bool>> CoinConfigurationStatus(int ccid)
        {
            return await _coinsConfigurationRepository.CoinConfigurationStatus(ccid);
        }

        public async Task<ServiceResponse<List<CoinConfigurationDTO>>> GetAllCoinConfigurations()
        {
            return await _coinsConfigurationRepository.GetAllCoinConfigurations();
        }

        public async Task<ServiceResponse<CoinConfigurationDTO>> GetCoinConfigurationByID(int ccid)
        {
            return await _coinsConfigurationRepository.GetCoinConfigurationByID(ccid);
        }
    }
}
