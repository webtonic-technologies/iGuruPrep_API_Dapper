using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request);
        Task<ServiceResponse<List<Notification>>> GetAllNotificationsList();
        Task<ServiceResponse<NotificationDTO>> GetNotificationById(int NotificationId);
    }
}
