using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Services.Interfaces
{
    public interface IContentIndexServices
    {
        Task<ServiceResponse<List<ContentIndexRequest>>> GetAllContentIndexList(ContentIndexListDTO request);
        Task<ServiceResponse<ContentIndexRequest>> GetContentIndexById(int id);
        Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndexRequest request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
