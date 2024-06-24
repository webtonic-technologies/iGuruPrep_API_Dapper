using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class ContentMasterServices : IContentMasterServices
    {
        private readonly IContentMasterRepository _contentMasterRepository;

        public ContentMasterServices(IContentMasterRepository contentMasterRepository)
        {
            _contentMasterRepository = contentMasterRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateContent(ContentMaster request)
        {
            try
            {
                return await _contentMasterRepository.AddUpdateContent(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<ContentMasterResponseDTO>> GetContentById(int ContentId)
        {
            try
            {
                return await _contentMasterRepository.GetContentById(ContentId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentMasterResponseDTO>(false, ex.Message, new ContentMasterResponseDTO(), 500);
            }
        }


        public async Task<ServiceResponse<List<ContentMasterResponseDTO>>> GetContentList(GetAllContentListRequest request)
        {
            try
            {
                return await _contentMasterRepository.GetContentList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentMasterResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexRequestDTO request)
        {
            try
            {
                return await _contentMasterRepository.GetAllContentIndexList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponse>>(false, ex.Message, [], 500);
            }
        }
    }
}
