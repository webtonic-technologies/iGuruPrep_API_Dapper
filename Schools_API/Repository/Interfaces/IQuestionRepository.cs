using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;

namespace Schools_API.Repository.Interfaces
{
    public interface IQuestionRepository
    {
        Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId);
        Task<ServiceResponse<List<QuestionDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<string>> AddQuestion(QuestionDTO request);
    }
}
