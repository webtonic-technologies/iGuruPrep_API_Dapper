using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Implementations
{
    public class ClassCourseService : IClassCourseService
    {
        private readonly IClassCourseRepository _classCourseRepository;

        public ClassCourseService(IClassCourseRepository classCourseRepository)
        {
            _classCourseRepository = classCourseRepository;
        }

        public async Task<ServiceResponse<List<GetClassCourseResponse>>> GetClassesByCourseIdAsync(int courseId)
        {
            return await _classCourseRepository.GetClassesByCourseIdAsync(courseId);
        }
    }
}
