using Course_API.DTOs;
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

        public async Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId)
        {
            try
            {
                return await _contentMasterRepository.GetContentById(ContentId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentMaster>(false, ex.Message, new ContentMaster(), 500);
            }
        }


        public async Task<ServiceResponse<List<ContentMaster>>> GetContentList(GetAllContentListRequest request)
        {
            try
            {
                return await _contentMasterRepository.GetContentList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentMaster>>(false, ex.Message, new List<ContentMaster>(), 500);
            }
        }

        public async Task<ServiceResponse<List<SubjectContentIndexDTO>>> GetListOfSubjectContent(SubjectContentIndexRequestDTO request)
        {
            try
            {
                return await _contentMasterRepository.GetListOfSubjectContent(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectContentIndexDTO>>(false, ex.Message, new List<SubjectContentIndexDTO>(), 500);
            }
        }
    }
}
