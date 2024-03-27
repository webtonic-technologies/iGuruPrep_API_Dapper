using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IQuestionLevelServices
    {
        Task<ServiceResponse<List<QuestionLevel>>> GetAllQuestionLevel();
        Task<ServiceResponse<QuestionLevel>> GetQuestionLevelById(int id);
        Task<ServiceResponse<string>> AddUpdateQuestionLevel(QuestionLevel request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
