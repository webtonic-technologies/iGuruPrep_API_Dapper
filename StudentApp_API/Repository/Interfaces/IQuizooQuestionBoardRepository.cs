using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IQuizooQuestionBoardRepository
    {
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuizQuestions(int quizooId, int registrationId);
        Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitAnswerAsync(List<SubmitAnswerRequest> requestList);
    }
}
