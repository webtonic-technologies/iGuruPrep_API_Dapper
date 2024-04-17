using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class StatusMessageServices : IStatusMessageServices
    {
        private readonly IStatusMessageRepository _statusMessageRepository;

        public StatusMessageServices(IStatusMessageRepository statusMessageRepository)
        {
            _statusMessageRepository = statusMessageRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateStatusMessage(StatusMessages request)
        {
            try
            {
                return await _statusMessageRepository.AddUpdateStatusMessage(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<StatusMessages>> GetStatusMessageById(int id)
        {
            try
            {
                return await _statusMessageRepository.GetStatusMessageById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StatusMessages>(false, ex.Message, new StatusMessages(), 500);
            }
        }

        public async Task<ServiceResponse<List<StatusMessages>>> GetStatusMessageList()
        {
            try
            {
                return await _statusMessageRepository.GetStatusMessageList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StatusMessages>>(false, ex.Message, [], 500);
            }
        }
    }
}
