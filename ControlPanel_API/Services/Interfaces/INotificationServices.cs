using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface INotificationServices
    {
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request);
        Task<ServiceResponse<List<Notification>>> GetAllNotificationsList();
        Task<ServiceResponse<NotificationDTO>> GetNotificationById(int NotificationId);
    }
}
