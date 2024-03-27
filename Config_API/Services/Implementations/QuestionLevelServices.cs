using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class QuestionLevelServices : IQuestionLevelServices
    {
        private readonly IQuestionLevelRepository  _questionLevelRepository;

        public QuestionLevelServices(IQuestionLevelRepository questionLevelRepository)
        {
            _questionLevelRepository = questionLevelRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionLevel(QuestionLevel request)
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

        public async Task<ServiceResponse<List<QuestionLevel>>> GetAllQuestionLevel()
        {
            try
            {
                return await _questionLevelRepository.GetAllQuestionLevel();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionLevel>>(false, ex.Message, new List<QuestionLevel>(), 500);
            }
        }

        public async Task<ServiceResponse<QuestionLevel>> GetQuestionLevelById(int id)
        {
            try
            {
                return await _questionLevelRepository.GetQuestionLevelById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionLevel>(false, ex.Message, new QuestionLevel(), 500);
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
