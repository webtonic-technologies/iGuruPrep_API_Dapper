using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;

namespace StudentApp_API.Services.Interfaces
{
    public interface ICourseService
    {
        Task<ServiceResponse<List<GetCourseResponse>>> GetCoursesAsync();
        Task<ServiceResponse<List<GetClassesResponse>>> GetClassesAsync();
        Task<ServiceResponse<List<GetBoardsResponse>>> GetBoardsAsync();
    }
}
