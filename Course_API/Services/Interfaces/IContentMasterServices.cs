using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Services.Interfaces
{
    public interface IContentMasterServices
    {
        Task<ServiceResponse<string>> AddUpdateContent(ContentMaster request);
        Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId);
        Task<ServiceResponse<List<ContentMaster>>> GetContentList();
        Task<ServiceResponse<List<SubjectContentIndexDTO>>> GetListOfSubjectContent(SubjectContentIndexRequestDTO request);
    }
}
