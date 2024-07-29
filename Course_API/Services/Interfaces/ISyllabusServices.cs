using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Services.Interfaces
{
    public interface ISyllabusServices
    {
        Task<ServiceResponse<int>> AddUpdateSyllabus(SyllabusDTO request);
        Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request);
        Task<ServiceResponse<SyllabusDetailsResponseDTO>> GetSyllabusDetailsById(int syllabusId, int subjectId);
        Task<ServiceResponse<SyllabusResponseDTO>> GetSyllabusById(int syllabusId);
        Task<ServiceResponse<string>> UpdateContentIndexName(UpdateContentIndexNameDTO request);
        Task<ServiceResponse<List<SyllabusResponseDTO>>> GetSyllabusList(GetAllSyllabusList request);
    }
}
