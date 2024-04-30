using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface INotificationModServices
    {
        Task<ServiceResponse<List<Module>>> GetAllModuleList();
        Task<ServiceResponse<List<Platform>>> GetAllPlatformList();
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationTemplate request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<NotificationModuleDTO>> GetNotificationsByModuleId(int id);
    }
}
