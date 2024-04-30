using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class DifficultyLevelServices : IDifficultyLevelServices
    {
        private readonly IDifficultyLevelRepository  _questionLevelRepository;

        public DifficultyLevelServices(IDifficultyLevelRepository questionLevelRepository)
        {
            _questionLevelRepository = questionLevelRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionLevel(DifficultyLevel request)
        {
            try
            {
                return await _questionLevelRepository.AddUpdateQuestionLevel(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevel()
        {
            try
            {
                return await _questionLevelRepository.GetAllQuestionLevel();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<DifficultyLevel>>(false, ex.Message, new List<DifficultyLevel>(), 500);
            }
        }

        public async Task<ServiceResponse<DifficultyLevel>> GetQuestionLevelById(int id)
        {
            try
            {
                return await _questionLevelRepository.GetQuestionLevelById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<DifficultyLevel>(false, ex.Message, new DifficultyLevel(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _questionLevelRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
