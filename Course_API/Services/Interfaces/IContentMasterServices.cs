using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Services.Interfaces
{
    public interface IContentMasterServices
    {
        Task<ServiceResponse<string>> AddUpdateContent(ContentMasterDTO request);
        Task<ServiceResponse<byte[]>> GetContentFileById(int ContentId);
        Task<ServiceResponse<byte[]>> GetContentFilePathUrlById(int ContentId);
        Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId);
        Task<ServiceResponse<List<ContentMaster>>> GetContentList();
    }
}
