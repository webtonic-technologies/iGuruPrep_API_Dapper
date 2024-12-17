using Quizoo_API.DTOs.Request;
using Quizoo_API.DTOs.Response;
using Quizoo_API.DTOs.ServiceResponse;
namespace Quizoo_API.Services.Interfaces
{
    public interface IQuizooServices
    {
        Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId);
        Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId);
        Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo);
        Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList);
        Task<ServiceResponse<List<QuizooDTO>>> GetQuizoosByRegistrationIdAsync(int registrationId);
        Task<ServiceResponse<List<QuizooDTO>>> GetInvitedQuizoosByRegistrationId(int registrationId);
    }
}
