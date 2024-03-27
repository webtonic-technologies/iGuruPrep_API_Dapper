using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class ClassCourseMappingServices : IClassCourseMappingServices
    {

        private readonly IClassCourseMappingRepository _classCourseMappingRepository;

        public ClassCourseMappingServices(IClassCourseMappingRepository classCourseMappingRepository)
        {
            _classCourseMappingRepository = classCourseMappingRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMapping request)
        {
            try
            {
                return await _classCourseMappingRepository.AddUpdateClassCourseMapping(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<ClassCourseMapping>>> GetAllClassCoursesMappings()
        {
            try
            {
                return await _classCourseMappingRepository.GetAllClassCoursesMappings();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMapping>>(false, ex.Message, new List<ClassCourseMapping>(), 500);
            }
        }

        public async Task<ServiceResponse<ClassCourseMapping>> GetClassCourseMappingById(int id)
        {
            try
            {
                return await _classCourseMappingRepository.GetClassCourseMappingById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ClassCourseMapping>(false, ex.Message, new ClassCourseMapping(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _classCourseMappingRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
