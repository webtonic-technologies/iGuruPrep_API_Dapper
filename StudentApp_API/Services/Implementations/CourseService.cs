using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<ServiceResponse<List<GetCourseResponse>>> GetCoursesAsync()
        {
            return await _courseRepository.GetAllCoursesAsync();
        }
    }
}
