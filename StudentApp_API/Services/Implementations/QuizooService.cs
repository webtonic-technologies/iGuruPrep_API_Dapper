using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
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
        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId)
        {
            return await _quizooRepository.GetChaptersAsync(registrationId, subjectId);
        }

        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(int registrationId)
        {
            return await _quizooRepository.GetInvitedQuizoosByRegistrationId(registrationId);
        }

        public async Task<ServiceResponse<QuizooDTOResponse>> GetQuizooByIdAsync(int quizooId)
        {
            return await _quizooRepository.GetQuizooByIdAsync(quizooId);
        }

        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(int registrationId)
        {
            return await _quizooRepository.GetQuizoosByRegistrationIdAsync(registrationId);
        }

        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            return await _quizooRepository.GetSubjectsAsync(registrationId);
        }

        public async Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo)
        {
            return await _quizooRepository.InsertOrUpdateQuizooAsync(quizoo);
        }

        public async Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList)
        {
            return await _quizooRepository.UpdateQuizooSyllabusAsync(quizooId, syllabusList);
        }
    }
}
