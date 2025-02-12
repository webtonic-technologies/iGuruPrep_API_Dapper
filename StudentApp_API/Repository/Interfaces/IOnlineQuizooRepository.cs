using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IOnlineQuizooRepository
    {
        Task<ServiceResponse<List<QuestionResponseDTO>>> InsertQuizooAsync(QuizooDTO quizoo);
        Task<ServiceResponse<List<QuestionWithCorrectAnswerDTO>>> GetQuestionsWithCorrectAnswersAsync(int quizooId);
        Task<ServiceResponse<List<StudentRankDTO>>> GetStudentRankListAsync(int quizooId, int userId);
        Task<ServiceResponse<int>> SetForceExitAsync(int qpid);
        Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int QuizooId);
    }
}
