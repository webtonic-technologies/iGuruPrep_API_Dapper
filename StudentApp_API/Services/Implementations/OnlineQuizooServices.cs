using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{

    public class OnlineQuizooServices : IOnlineQuizooServices
    {
        private readonly IOnlineQuizooRepository _onlineQuizooRepository;

        public OnlineQuizooServices(IOnlineQuizooRepository onlineQuizooRepository)
        {
            _onlineQuizooRepository = onlineQuizooRepository;
        }
        public async Task<ServiceResponse<List<QuestionWithCorrectAnswerDTO>>> GetQuestionsWithCorrectAnswersAsync(int quizooId)
        {
            return await _onlineQuizooRepository.GetQuestionsWithCorrectAnswersAsync(quizooId);
        }

        public async Task<ServiceResponse<List<StudentRankDTO>>> GetStudentRankListAsync(int quizooId, int userId)
        {
            return await _onlineQuizooRepository.GetStudentRankListAsync(quizooId, userId);
        }

        public async Task<ServiceResponse<List<QuestionResponseDTO>>> InsertQuizooAsync(OnlineQuizooDTO quizoo)
        {
            return await _onlineQuizooRepository.InsertQuizooAsync(quizoo);
        }

        public async Task<ServiceResponse<int>> SetForceExitAsync(int QuizooID, int StudentID)
        {
            return await _onlineQuizooRepository.SetForceExitAsync(QuizooID, StudentID);
        }
    }
}
