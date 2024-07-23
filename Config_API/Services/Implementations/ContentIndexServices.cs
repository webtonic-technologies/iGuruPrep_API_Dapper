using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class ContentIndexServices : IContentIndexServices
    {
        private readonly IContentIndexRepository _contentIndexRepository;

        public ContentIndexServices(IContentIndexRepository contentIndexRepository)
        {
            _contentIndexRepository = contentIndexRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndexRequest request)
        {
            try
            {
                return await _contentIndexRepository.AddUpdateContentIndex(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> AddUpdateContentIndexChapter(ContentIndexRequestdto request)
        {

            try
            {
                return await _contentIndexRepository.AddUpdateContentIndexChapter(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> AddUpdateContentIndexSubTopics(ContentIndexSubTopic request)
        {
            try
            {
                return await _contentIndexRepository.AddUpdateContentIndexSubTopics(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> AddUpdateContentIndexTopics(ContentIndexTopicsdto request)
        {
            {
                try
                {
                    return await _contentIndexRepository.AddUpdateContentIndexTopics(request);
                }
                catch (Exception ex)
                {
                    return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
                }
            }
        }

        public async Task<ServiceResponse<byte[]>> DownloadContentIndexBySubjectId(int subjectId)
        {
            {
                try
                {
                    return await _contentIndexRepository.DownloadContentIndexBySubjectId(subjectId);
                }
                catch (Exception ex)
                {
                    return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
                }
            }
        }

        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {
                return await _contentIndexRepository.GetAllContentIndexList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponse>> (false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexListMasters(ContentIndexMastersDTO request)
        {
            try
            {
                return await _contentIndexRepository.GetAllContentIndexListMasters(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponse>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<ContentIndexResponse>> GetContentIndexById(int id)
        {
            try
            {
                return await _contentIndexRepository.GetContentIndexById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentIndexResponse>(false, ex.Message, new ContentIndexResponse(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _contentIndexRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<string>> UploadContentIndex(IFormFile file)
        {
            try
            {
                return await _contentIndexRepository.UploadContentIndex(file);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
