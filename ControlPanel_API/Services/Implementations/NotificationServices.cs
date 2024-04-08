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
        public async Task<ServiceResponse<int>> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                return await _notificationRepository.AddUpdateNotification(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }

        public async Task<ServiceResponse<List<Notification>>> GetAllNotificationsList()
        {
            try
            {
                return await _notificationRepository.GetAllNotificationsList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Notification>>(false, ex.Message, new List<Notification>(), 500);
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

        public async Task<ServiceResponse<byte[]>> GetNotificationFileById(int NotificationId)
        {
            try
            {
                return await _notificationRepository.GetNotificationFileById(NotificationId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, Array.Empty<byte>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateNotificationFile(NotificationImageDTO request)
        {
            try
            {
                return await _notificationRepository.UpdateNotificationFile(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
