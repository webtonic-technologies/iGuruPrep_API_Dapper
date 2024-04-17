using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;

namespace Schools_API.Repository.Interfaces
{
    public interface IQuestionRepository
    {
        Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId);
        Task<ServiceResponse<List<Question>>> GetAllQuestionsList();
        Task<ServiceResponse<string>> AddQuestion(QuestionDTO request);
        Task<ServiceResponse<string>> UpdateQuestionImageFile(QuestionImageDTO request);
    }
}
