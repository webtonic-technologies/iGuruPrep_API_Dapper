using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Repository.Interfaces
{
    public interface IContentMasterRepository
    {
        Task<ServiceResponse<string>> AddUpdateContent(ContentMasterDTO request);
        Task<ServiceResponse<byte[]>> GetContentFileById(int ContentId);
        Task<ServiceResponse<byte[]>> GetContentFilePathUrlById(int ContentId);
        Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId);
        Task<ServiceResponse<List<ContentMaster>>> GetContentList();
        Task<ServiceResponse<List<SubjectContentIndexDTO>>> GetListOfSubjectContent(SubjectContentIndexRequestDTO request);
    }
}
