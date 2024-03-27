using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IStatusMessageServices
    {
        Task<ServiceResponse<StatusMessages>> GetStatusMessageById(int id);
        Task<ServiceResponse<string>> AddUpdateStatusMessage(StatusMessages request);
    }
}
