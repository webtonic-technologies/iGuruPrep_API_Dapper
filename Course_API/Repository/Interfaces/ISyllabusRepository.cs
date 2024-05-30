using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Repository.Interfaces
{
    public interface ISyllabusRepository
    {
        Task<ServiceResponse<string>> AddUpdateSyllabus(SyllabusDTO request);
        Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request);
    }
}
