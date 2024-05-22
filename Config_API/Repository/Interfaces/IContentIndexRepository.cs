using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IContentIndexRepository
    {
        Task<ServiceResponse<List<ContentIndex>>> GetAllContentIndexList(ContentIndexListDTO request);
        Task<ServiceResponse<ContentIndex>> GetContentIndexById(int id);
        Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndex request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
