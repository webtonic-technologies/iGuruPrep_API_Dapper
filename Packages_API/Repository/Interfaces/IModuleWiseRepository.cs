using Packages_API.DTOs.ServiceResponse;
using Packages_API.DTOs.Response;

namespace Packages_API.Repository.Interfaces
{
    public interface IModuleWiseRepository
    {
        Task<ServiceResponse<List<ModuleDTO>>> GetModules();
        Task<ServiceResponse<bool>> SetModuleWiseConfiguration(List<ModuleWiseConfigDTO> configs);
        Task<ServiceResponse<List<ModuleWiseConfigDTO>>> GetModuleWiseConfiguration();
    }
}
