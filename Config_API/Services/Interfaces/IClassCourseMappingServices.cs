using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IClassCourseMappingServices
    {
        Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request);
        Task<ServiceResponse<ClassCourseMappingResponse>> GetClassCourseMappingById(int id);
        Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
