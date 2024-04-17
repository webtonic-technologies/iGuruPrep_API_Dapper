using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;

namespace Schools_API.Services.Interfaces
{
    public interface IQuestionServices
    {
        Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId);
        Task<ServiceResponse<List<Question>>> GetAllQuestionsList();
        Task<ServiceResponse<string>> AddQuestion(QuestionDTO request);
        Task<ServiceResponse<string>> UpdateQuestionImageFile(QuestionImageDTO request);
    }
}
