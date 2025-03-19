using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class QuizooService : IQuizooServices
    {
        private readonly IQuizooRepository _quizooRepository;

        public QuizooService(IQuizooRepository quizooRepository)
        {
            _quizooRepository = quizooRepository;
        }

        public async Task<ServiceResponse<bool>> CheckAndDismissQuizAsync(int quizooId)
        {
            return await _quizooRepository.CheckAndDismissQuizAsync(quizooId);
        }

        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId)
        {
            return await _quizooRepository.GetChaptersAsync(registrationId, subjectId);
        }

        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(QuizooListFilters request)
        {
            return await _quizooRepository.GetInvitedQuizoosByRegistrationId(request);
        }

        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetOnlineQuizoosByRegistrationIdAsync(QuizooListFilters request)
        {
            return await _quizooRepository.GetOnlineQuizoosByRegistrationIdAsync(request);
        }

        public async Task<ServiceResponse<List<ParticipantDto>>> GetParticipantsAsync(int quizooId, int studentId)
        {
            return await _quizooRepository.GetParticipantsAsync(quizooId, studentId);
        }

        public async Task<ServiceResponse<QuizooDTOResponse>> GetQuizooByIdAsync(int quizooId)
        {
            return await _quizooRepository.GetQuizooByIdAsync(quizooId);
        }

        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(QuizooListFilters request)
        {
            return await _quizooRepository.GetQuizoosByRegistrationIdAsync(request);
        }

        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            return await _quizooRepository.GetSubjectsAsync(registrationId);
        }

        public async Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo)
        {
            return await _quizooRepository.InsertOrUpdateQuizooAsync(quizoo);
        }

        public async Task<ServiceResponse<int>> SetForceExitAsync(int QuizooID, int StudentID)
        {
            return await _quizooRepository.SetForceExitAsync(QuizooID, StudentID);
        }

        public async Task<ServiceResponse<string>> ShareQuizooAsync(int studentId, int quizooId)
        {
            return await _quizooRepository.ShareQuizooAsync(studentId, quizooId);
        }

        public async Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList)
        {
            return await _quizooRepository.UpdateQuizooSyllabusAsync(quizooId, syllabusList);
        }

        public async Task<ServiceResponse<string>> ValidateQuizStartAsync(int quizooId, int studentId)
        {
            return await _quizooRepository.ValidateQuizStartAsync(quizooId, studentId);
        }
    }
}
