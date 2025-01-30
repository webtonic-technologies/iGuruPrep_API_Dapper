using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class ConceptwisePracticeServices : IConceptwisePracticeServices
    {
        private readonly IConceptwisePracticeRepository _conceptwisePracticeRepository;

        public ConceptwisePracticeServices(IConceptwisePracticeRepository conceptwisePracticeRepository)
        {
            _conceptwisePracticeRepository = conceptwisePracticeRepository;
        }

        public async Task<ServiceResponse<AccuracyRateDtoComparison>> GetAccuracyRates(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetAccuracyRates(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<AnswerTimeStatsDto>> GetAnswerTimeStats(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetAnswerTimeStats(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<double>> GetAverageTimeSpentByOtherStudents(int studentId, int questionId)
        {
            return await _conceptwisePracticeRepository.GetAverageTimeSpentByOtherStudents(studentId, questionId);
        }

        public async Task<ServiceResponse<double>> GetAverageTimeSpentOnQuestion(int studentId, int questionId)
        {
            return await _conceptwisePracticeRepository.GetAverageTimeSpentOnQuestion(studentId, questionId);
        }

        public async Task<ServiceResponse<List<StudentAccuracyDto>>> GetClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetClassmatesAccuracyRate(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<PracticeStatsDto>> GetPracticeQuestionStats(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetPracticeQuestionStats(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<QuestionAttemptStatsResponse>> GetQuestionAttemptStatsForGroupAsync(int studentId, int questionId)
        {
            return await _conceptwisePracticeRepository.GetQuestionAttemptStatsForGroupAsync(studentId, questionId);
        }

        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsAsync(GetQuestionsList request)
        {
            return await _conceptwisePracticeRepository.GetQuestionsAsync(request);
        }

        public async Task<ServiceResponse<AccuracyRateDto>> GetStudentAndClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetStudentAndClassmatesAccuracyRate(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<decimal>> GetStudentGroupAccuracyForQuestionAsync(int studentId, int questionId)
        {
            return await _conceptwisePracticeRepository.GetStudentGroupAccuracyForQuestionAsync(studentId, questionId);
        }

        public async Task<ServiceResponse<decimal>> GetStudentQuestionAccuracyAsync(int studentId, int questionId)
        {
            return await _conceptwisePracticeRepository.GetStudentQuestionAccuracyAsync(studentId, questionId);
        }

        public async Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            return await _conceptwisePracticeRepository.GetSyllabusContentDetails(request);
        }

        public async Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId)
        {
            return await _conceptwisePracticeRepository.GetSyllabusSubjects(RegistrationId);
        }

        public async Task<ServiceResponse<TimeSpentDto>> GetTotalAndAverageTimeSpent(int studentId, int indexTypeId, int contentId, int syllabusId)
        {
            return await _conceptwisePracticeRepository.GetTotalAndAverageTimeSpent(studentId, indexTypeId, contentId, syllabusId);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request)
        {
            return await _conceptwisePracticeRepository.MarkQuestionAsSave(request);
        }

        public async Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            return await _conceptwisePracticeRepository.SubmitAnswerAsync(request);
        }
    }
}
