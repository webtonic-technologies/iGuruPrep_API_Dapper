using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IClassCourseMappingRepository
    {
        Task<ServiceResponse<List<ClassCourseMappingDTO>>> GetAllClassCoursesMappings();
        Task<ServiceResponse<ClassCourseMappingDTO>> GetClassCourseMappingById(int id);
        Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
