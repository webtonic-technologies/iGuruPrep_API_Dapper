using Config_API.DTOs.ServiceResponse;
using iGuruPrep.Models;

namespace Config_API.Services.Interfaces
{
    public interface IClassServices
    {
        Task<ServiceResponse<List<Class>>> GetAllClasses();
        Task<ServiceResponse<Class>> GetClassById(int id);
        Task<ServiceResponse<string>> AddUpdateClass(Class request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
