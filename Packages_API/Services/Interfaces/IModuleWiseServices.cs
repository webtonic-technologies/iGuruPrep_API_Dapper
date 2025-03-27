using Packages_API.DTOs.ServiceResponse;
using Packages_API.DTOs.Response;

namespace Packages_API.Services.Interfaces
{
    public interface IModuleWiseServices
    {
        Task<ServiceResponse<List<ModuleDTO>>> GetModules();
        Task<ServiceResponse<bool>> SetModuleWiseConfiguration(List<ModuleWiseConfigDTO> configs);
        Task<ServiceResponse<List<ModuleWiseConfigDTO>>> GetModuleWiseConfiguration();
    }
}
