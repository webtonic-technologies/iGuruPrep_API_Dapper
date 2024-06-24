using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Services.Interfaces
{
    public interface IContentMasterServices
    {
        Task<ServiceResponse<string>> AddUpdateContent(ContentMaster request);
        Task<ServiceResponse<ContentMasterResponseDTO>> GetContentById(int ContentId);
        Task<ServiceResponse<List<ContentMasterResponseDTO>>> GetContentList(GetAllContentListRequest request);
        Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexRequestDTO request);
    }
}
