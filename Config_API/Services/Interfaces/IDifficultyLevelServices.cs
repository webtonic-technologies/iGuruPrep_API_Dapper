using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IDifficultyLevelServices
    {
        Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevel();
        Task<ServiceResponse<DifficultyLevel>> GetQuestionLevelById(int id);
        Task<ServiceResponse<string>> AddUpdateQuestionLevel(DifficultyLevel request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
