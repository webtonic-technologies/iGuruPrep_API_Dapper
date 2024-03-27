using Config_API.DTOs.ServiceResponse;
using iGuruPrep.Models;

namespace Config_API.Services.Interfaces
{
    public interface ISubjectServices
    {
        Task<ServiceResponse<List<Subject>>> GetAllSubjects();
        Task<ServiceResponse<Subject>> GetSubjectById(int id);
        Task<ServiceResponse<string>> AddUpdateSubject(Subject request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
