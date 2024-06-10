using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;
using iGuruPrep.Models;

namespace Config_API.Services.Implementations
{
    public class ClassServices : IClassServices
    {
        private readonly IClassRepository _classRepository;

        public ClassServices(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateClass(Class request)
        {
            try
            {
                return await _classRepository.AddUpdateClass(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Class>>> GetAllClasses(GetAllClassesRequest request)
        {
            try
            {
                return await _classRepository.GetAllClasses(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<Class>>> GetAllClassesMaster()
        {
            try
            {
                return await _classRepository.GetAllClassesMaster();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<Class>> GetClassById(int id)
        {
            try
            {
                return await _classRepository.GetClassById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Class>(false, ex.Message, new Class(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _classRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
