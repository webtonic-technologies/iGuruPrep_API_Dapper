using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
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
        public async Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request)
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

        public async Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request)
        {
            try
            {
                return await _classCourseMappingRepository.GetAllClassCoursesMappings(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMappingResponse>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<ClassCourseMappingResponse>> GetClassCourseMappingById(int id)
        {
            try
            {
                return await _classCourseMappingRepository.GetClassCourseMappingById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ClassCourseMappingResponse>(false, ex.Message, new ClassCourseMappingResponse(), 500);
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
