using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IQuestionLevelRepository
    {

        Task<ServiceResponse<List<QuestionLevel>>> GetAllQuestionLevel();
        Task<ServiceResponse<QuestionLevel>> GetQuestionLevelById(int id);
        Task<ServiceResponse<string>> AddUpdateQuestionLevel(QuestionLevel request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
