using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class NotificationServices : INotificationServices
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationServices(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                return await _notificationRepository.AddUpdateNotification(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<NotificationDTO>>> GetAllNotificationsList(NotificationsListDTO request)
        {
            try
            {
                return await _notificationRepository.GetAllNotificationsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NotificationDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<NotificationDTO>> GetNotificationById(int NotificationId)
        {
            try
            {
                return await _notificationRepository.GetNotificationById(NotificationId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<NotificationDTO>(false, ex.Message, new NotificationDTO(), 500);
            }
        }
    }
}
