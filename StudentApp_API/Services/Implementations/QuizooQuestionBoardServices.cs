using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class QuizooQuestionBoardServices : IQuizooQuestionBoardServices
    {
        private readonly IQuizooQuestionBoardRepository _quizooQuestionBoardRepository;

        public QuizooQuestionBoardServices(IQuizooQuestionBoardRepository quizooQuestionBoardRepository)
        {
            _quizooQuestionBoardRepository = quizooQuestionBoardRepository;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuizQuestions(int quizooId, int registrationId)
        {
            return await _quizooQuestionBoardRepository.GetQuizQuestions(quizooId, registrationId);
        }

        public async Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitAnswerAsync(List<SubmitAnswerRequest> requestList)
        {
            return await _quizooQuestionBoardRepository.SubmitAnswerAsync(requestList);
        }
    }
}
