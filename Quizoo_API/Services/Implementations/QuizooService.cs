using Quizoo_API.DTOs.Request;
using Quizoo_API.DTOs.Response;
using Quizoo_API.DTOs.ServiceResponse;
using Quizoo_API.Repository.Interfaces;
using Quizoo_API.Services.Interfaces;

namespace Quizoo_API.Services.Implementations
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

        public async Task<ServiceResponse<List<QuizooDTO>>> GetInvitedQuizoosByRegistrationId(int registrationId)
        {
            return await _quizooRepository.GetInvitedQuizoosByRegistrationId(registrationId);
        }

        public async Task<ServiceResponse<List<QuizooDTO>>> GetQuizoosByRegistrationIdAsync(int registrationId)
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
