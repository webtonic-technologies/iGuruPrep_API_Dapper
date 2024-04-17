using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Implementations;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class NotificationModServices : INotificationModServices
    {
        private readonly INotificationModRepository _notificationModRepository;

        public NotificationModServices(INotificationModRepository notificationModRepository)
        {
            _notificationModRepository = notificationModRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationTemplate request)
        {
            try
            {
                return await _notificationModRepository.AddUpdateNotification(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<ModuleNew>>> GetAllModuleList()
        {
            try
            {
                return await _notificationModRepository.GetAllModuleList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ModuleNew>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<Platform>>> GetAllPlatformList()
        {
            try
            {
                return await _notificationModRepository.GetAllPlatformList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Platform>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<NotificationModuleDTO>> GetNotificationsByModuleId(int id)
        {
            try
            {
                return await _notificationModRepository.GetNotificationsByModuleId(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationModuleDTO>(false, ex.Message, new NotificationModuleDTO(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _notificationModRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
