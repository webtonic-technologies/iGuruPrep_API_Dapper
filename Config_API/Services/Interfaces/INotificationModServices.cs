using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface INotificationModServices
    {
        Task<ServiceResponse<List<NotificationModule>>> GetAllModuleList();
        Task<ServiceResponse<List<Platform>>> GetAllPlatformList();
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationDTO request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<NotificationResponseDTO>> GetNotificationsById(int id);
        Task<ServiceResponse<List<NotificationResponseDTO>>> GetListofNotifications();
    }
}
