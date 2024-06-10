using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IDifficultyLevelRepository
    {

        Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevel(GetAllDifficultyLevelRequest request);
        Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevelMasters();
        Task<ServiceResponse<DifficultyLevel>> GetQuestionLevelById(int id);
        Task<ServiceResponse<string>> AddUpdateQuestionLevel(DifficultyLevel request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
