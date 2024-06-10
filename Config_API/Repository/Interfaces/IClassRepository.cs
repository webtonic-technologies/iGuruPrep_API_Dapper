using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using iGuruPrep.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IClassRepository
    {
        Task<ServiceResponse<List<Class>>> GetAllClasses(GetAllClassesRequest request);
        Task<ServiceResponse<List<Class>>> GetAllClassesMaster();
        Task<ServiceResponse<Class>> GetClassById(int id);
        Task<ServiceResponse<string>> AddUpdateClass(Class request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
