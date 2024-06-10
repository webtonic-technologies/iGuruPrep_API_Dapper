using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using iGuruPrep.Models;

namespace Config_API.Repository.Interfaces
{
    public interface ICourseRepository
    {
        Task<ServiceResponse<List<Course>>> GetAllCourses(GetAllCoursesRequest request);
        Task<ServiceResponse<List<Course>>> GetAllCoursesMasters();
        Task<ServiceResponse<Course>> GetCourseById(int id);
        Task<ServiceResponse<string>> AddUpdateCourse(Course request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
