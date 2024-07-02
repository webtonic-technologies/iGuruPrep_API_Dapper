using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request);
        Task<ServiceResponse<List<NotificationResponseDTO>>> GetAllNotificationsList(NotificationsListDTO request);
        Task<ServiceResponse<NotificationResponseDTO>> GetNotificationById(int NotificationId);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
