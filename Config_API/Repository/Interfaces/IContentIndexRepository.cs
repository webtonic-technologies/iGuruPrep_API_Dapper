using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IContentIndexRepository
    {
        Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexListDTO request);
        Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexListMasters(ContentIndexMastersDTO request);
        Task<ServiceResponse<ContentIndexResponse>> GetContentIndexById(int id);
        Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndexRequest request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<string>> AddUpdateContentIndexSubTopics(ContentIndexSubTopic request);
        Task<ServiceResponse<string>> AddUpdateContentIndexTopics(ContentIndexTopicsdto request);
        Task<ServiceResponse<string>> AddUpdateContentIndexChapter(ContentIndexRequestdto request);
        Task<ServiceResponse<byte[]>> DownloadContentIndexBySubjectId(int subjectId);
        Task<ServiceResponse<string>> UploadContentIndex(IFormFile file);
    }
}
