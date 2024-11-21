using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Interfaces
{
    public interface ICourseRepository
    {
        Task<ServiceResponse<List<GetCourseResponse>>> GetAllCoursesAsync();
        Task<ServiceResponse<List<GetClassesResponse>>> GetClassesAsync();
        Task<ServiceResponse<List<GetBoardsResponse>>> GetBoardsAsync();
    }
}
