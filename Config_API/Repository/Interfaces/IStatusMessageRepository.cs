using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IStatusMessageRepository
    {
        Task<ServiceResponse<StatusMessages>> GetStatusMessageById(int id);
        Task<ServiceResponse<string>> AddUpdateStatusMessage(StatusMessages request);
        Task<ServiceResponse<List<StatusMessages>>> GetStatusMessageList(GetAllStatusMessagesRequest request);
    }
}
