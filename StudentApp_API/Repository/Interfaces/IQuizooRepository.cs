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
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(QuizooListFilters request);
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(QuizooListFilters request);
        Task<ServiceResponse<QuizooDTOResponse>> GetQuizooByIdAsync(int quizooId);
        Task<ServiceResponse<List<QuizooDTOResponse>>> GetOnlineQuizoosByRegistrationIdAsync(QuizooListFilters request);
        Task<ServiceResponse<string>> ShareQuizooAsync(int studentId, int quizooId);
        Task<ServiceResponse<string>> ValidateQuizStartAsync(int quizooId, int studentId);
        Task<ServiceResponse<bool>> CheckAndDismissQuizAsync(int quizooId);
        Task<ServiceResponse<List<ParticipantDto>>> GetParticipantsAsync(int quizooId, int studentId);
        Task<ServiceResponse<int>> SetForceExitAsync(int QuizooID, int StudentID);
    }
}
