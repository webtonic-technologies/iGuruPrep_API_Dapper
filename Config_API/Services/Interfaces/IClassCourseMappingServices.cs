using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IClassCourseMappingServices
    {
        Task<ServiceResponse<List<ClassCourseMapping>>> GetAllClassCoursesMappings();
        Task<ServiceResponse<ClassCourseMapping>> GetClassCourseMappingById(int id);
        Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMapping request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
