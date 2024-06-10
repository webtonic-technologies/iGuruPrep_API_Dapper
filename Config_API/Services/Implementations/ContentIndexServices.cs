using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
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

        public async Task<ServiceResponse<List<ContentIndexRequest>>> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {
                return await _contentIndexRepository.GetAllContentIndexList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexRequest>> (false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<ContentIndexRequest>> GetContentIndexById(int id)
        {
            try
            {
                return await _contentIndexRepository.GetContentIndexById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentIndexRequest>(false, ex.Message, new ContentIndexRequest(), 500);
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
    }
}
