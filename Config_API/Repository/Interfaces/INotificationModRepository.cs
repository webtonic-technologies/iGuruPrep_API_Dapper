using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface INotificationModRepository
    {
        Task<ServiceResponse<List<ModuleNew>>> GetAllModuleList();
        Task<ServiceResponse<List<Platform>>> GetAllPlatformList();
        Task<ServiceResponse<string>> AddUpdateNotification(NotificationTemplate request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<NotificationModuleDTO>> GetNotificationsByModuleId(int id);
    }
}
