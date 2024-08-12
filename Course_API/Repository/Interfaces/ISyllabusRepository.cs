using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Repository.Interfaces
{
    public interface ISyllabusRepository
    {
        Task<ServiceResponse<int>> AddUpdateSyllabus(SyllabusDTO request);
        Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request);
        Task<ServiceResponse<SyllabusDetailsResponse>> GetSyllabusDetailsById(int syllabusId, int subjectId);
        Task<ServiceResponse<SyllabusResponseDTO>> GetSyllabusById(int syllabusId);
        Task<ServiceResponse<string>> UpdateContentIndexName(UpdateContentIndexNameDTO request);
        Task<ServiceResponse<List<SyllabusResponseDTO>>> GetSyllabusList(GetAllSyllabusList request);
        Task<ServiceResponse<List<ContentIndexResponses>>> GetAllContentIndexList(int SubjectId);
    }
}
