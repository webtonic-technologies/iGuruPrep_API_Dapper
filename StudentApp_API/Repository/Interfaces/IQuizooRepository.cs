using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IQuizooRepository
    {
        Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId);
        Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId);
        Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo);
        Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList);
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(int registrationId);
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(int registrationId);
        Task<ServiceResponse<QuizooDTOResponse>> GetQuizooByIdAsync(int quizooId);
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetOnlineQuizoosByRegistrationIdAsync(int registrationId);
    }
}
