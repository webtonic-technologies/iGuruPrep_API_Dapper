using Packages_API.DTOs.ServiceResponse;
using Packages_API.DTOs.Requests;

namespace Packages_API.Repository.Interfaces
{
    public interface ICoinsConfigurationRepository
    {
        Task<ServiceResponse<bool>> AddUpdateCoinConfiguration(AddUpdateCoinConfigurationRequest request);
        Task<ServiceResponse<List<CoinConfigurationDTO>>> GetAllCoinConfigurations();
        Task<ServiceResponse<CoinConfigurationDTO>> GetCoinConfigurationByID(int ccid);
        Task<ServiceResponse<bool>> CoinConfigurationStatus(int ccid);
    }
}
