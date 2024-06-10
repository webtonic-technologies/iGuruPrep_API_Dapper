using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using iGuruPrep.Models;

namespace Config_API.Repository.Interfaces
{
    public interface ISubjectRepository
    {
        Task<ServiceResponse<List<Subject>>> GetAllSubjects(GetAllSubjectsRequest request);
        Task<ServiceResponse<List<Subject>>> GetAllSubjectsMAsters();
        Task<ServiceResponse<Subject>> GetSubjectById(int id);
        Task<ServiceResponse<string>> AddUpdateSubject(Subject request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
