using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;
using iGuruPrep.Models;

namespace Config_API.Services.Implementations
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepository _courseRepository;

        public CourseServices(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateCourse(Course request)
        {
            try
            {
                return await _courseRepository.AddUpdateCourse(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Course>>> GetAllCourses(GetAllCoursesRequest request)
        {
            try
            {
                return await _courseRepository.GetAllCourses(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Course>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<Course>>> GetAllCoursesMasters()
        {
            try
            {
                return await _courseRepository.GetAllCoursesMasters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Course>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<Course>> GetCourseById(int id)
        {
            try
            {
                return await _courseRepository.GetCourseById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Course>(false, ex.Message, new Course(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _courseRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
