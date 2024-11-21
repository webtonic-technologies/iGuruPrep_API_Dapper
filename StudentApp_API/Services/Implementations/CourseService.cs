using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<ServiceResponse<List<GetBoardsResponse>>> GetBoardsAsync()
        {
            return await _courseRepository.GetBoardsAsync();
        }

        public async Task<ServiceResponse<List<GetClassesResponse>>> GetClassesAsync()
        {
            return await _courseRepository.GetClassesAsync();
        }

        public async Task<ServiceResponse<List<GetCourseResponse>>> GetCoursesAsync()
        {
            return await _courseRepository.GetAllCoursesAsync();
        }
    }
}
