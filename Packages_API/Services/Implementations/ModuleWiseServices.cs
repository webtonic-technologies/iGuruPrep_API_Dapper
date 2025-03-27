using Packages_API.DTOs.Response;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Repository.Interfaces;
using Packages_API.Services.Interfaces;

namespace Packages_API.Services.Implementations
{
    public class ModuleWiseServices : IModuleWiseServices
    {
        private readonly IModuleWiseRepository _moduleWiseRepository;

        public ModuleWiseServices(IModuleWiseRepository moduleWiseRepository)
        {
            _moduleWiseRepository = moduleWiseRepository;
        }

        public async Task<ServiceResponse<List<ModuleDTO>>> GetModules()
        {
            return await _moduleWiseRepository.GetModules();
        }

        public async Task<ServiceResponse<List<ModuleWiseConfigDTO>>> GetModuleWiseConfiguration()
        {
            return await _moduleWiseRepository.GetModuleWiseConfiguration();
        }

        public async Task<ServiceResponse<bool>> SetModuleWiseConfiguration(List<ModuleWiseConfigDTO> configs)
        {
            return await _moduleWiseRepository.SetModuleWiseConfiguration(configs);
        }
    }
}