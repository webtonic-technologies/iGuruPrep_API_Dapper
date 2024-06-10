using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Repository.Interfaces
{
    public interface IClassCourseMappingRepository
    {
        Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request);
        Task<ServiceResponse<ClassCourseMappingResponse>> GetClassCourseMappingById(int id);
        Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
