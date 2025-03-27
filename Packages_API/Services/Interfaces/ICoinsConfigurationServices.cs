using Packages_API.DTOs.ServiceResponse;
using Packages_API.DTOs.Requests;

namespace Packages_API.Services.Interfaces
{
    public interface ICoinsConfigurationServices
    {
        Task<ServiceResponse<bool>> AddUpdateCoinConfiguration(AddUpdateCoinConfigurationRequest request);
        Task<ServiceResponse<List<CoinConfigurationDTO>>> GetAllCoinConfigurations();
        Task<ServiceResponse<CoinConfigurationDTO>> GetCoinConfigurationByID(int ccid);
        Task<ServiceResponse<bool>> CoinConfigurationStatus(int ccid);
    }
}
