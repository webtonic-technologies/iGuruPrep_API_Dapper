using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Services.Interfaces
{
    public interface ISyllabusServices
    {
        Task<ServiceResponse<int>> AddUpdateSyllabus(SyllabusDTO request);
        Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request);
        Task<ServiceResponse<SyllabusDetailsResponse>> GetSyllabusDetailsById(int syllabusId, int subjectId);
        Task<ServiceResponse<SyllabusResponseDTO>> GetSyllabusById(int syllabusId);
        Task<ServiceResponse<string>> UpdateContentIndexName(UpdateContentIndexNameDTO request);
        Task<ServiceResponse<List<SyllabusResponseDTO>>> GetSyllabusList(GetAllSyllabusList request);
        Task<ServiceResponse<List<ContentIndexResponses>>> GetAllContentIndexList(int SubjectId);
        Task<ServiceResponse<byte[]>> DownloadExcelFile(int SyllabusId);
        Task<ServiceResponse<string>> UploadSyllabusDetails(IFormFile file);
    }
}
