using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class SyllabusServices : ISyllabusServices
    {
        private readonly ISyllabusRepository _syllabusRepository;

        public SyllabusServices(ISyllabusRepository syllabusRepository)
        {
            _syllabusRepository = syllabusRepository;
        }
        public async Task<ServiceResponse<int>> AddUpdateSyllabus(SyllabusDTO request)
        {
            try
            {
                return await _syllabusRepository.AddUpdateSyllabus(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }

        public async Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request)
        {
            try
            {
                return await _syllabusRepository.AddUpdateSyllabusDetails(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<byte[]>> DownloadExcelFile(int SyllabusId)
        {
            try
            {
                return await _syllabusRepository.DownloadExcelFile(SyllabusId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetAllContentIndexList(int SubjectId)
        {
            try
            {
                return await _syllabusRepository.GetAllContentIndexList(SubjectId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<SyllabusResponseDTO>> GetSyllabusById(int syllabusId)
        {
            try
            {
                return await _syllabusRepository.GetSyllabusById(syllabusId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SyllabusResponseDTO>(false, ex.Message, new SyllabusResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<SyllabusDetailsResponse>> GetSyllabusDetailsById(int syllabusId, int subjectId)
        {
            try
            {
                return await _syllabusRepository.GetSyllabusDetailsById(syllabusId, subjectId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SyllabusDetailsResponse>(false, ex.Message, new SyllabusDetailsResponse(), 500);
            }
        }

        public async Task<ServiceResponse<List<SyllabusResponseDTO>>> GetSyllabusList(GetAllSyllabusList request)
        {
            try
            {
                return await _syllabusRepository.GetSyllabusList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SyllabusResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateContentIndexName(UpdateContentIndexNameDTO request)
        {
            try
            {
                return await _syllabusRepository.UpdateContentIndexName(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> UploadSyllabusDetails(IFormFile file)
        {
            try
            {
                return await _syllabusRepository.UploadSyllabusDetails(file);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
