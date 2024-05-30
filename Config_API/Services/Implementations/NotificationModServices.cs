using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
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
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request)
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

        public async Task<ServiceResponse<List<NotificationModule>>> GetAllModuleList()
        {
            try
            {
                return await _notificationModRepository.GetAllModuleList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationModule>>(false, ex.Message, [], 500);
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

        public async Task<ServiceResponse<List<NotificationResponseDTO>>> GetListofNotifications()
        {
            try
            {
                return await _notificationModRepository.GetListofNotifications();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<NotificationResponseDTO>> GetNotificationsById(int id)
        {
            try
            {
                return await _notificationModRepository.GetNotificationsById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationResponseDTO>(false, ex.Message, new NotificationResponseDTO(), 500);
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
