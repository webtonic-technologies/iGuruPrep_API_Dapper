using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
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

        public async Task<ServiceResponse<ChapterAccuracyReportResponse>> GetChapterAccuracyReportAsync(ChapterAnalyticsRequest request)
        {
            return await _conceptwisePracticeRepository.GetChapterAccuracyReportAsync(request);
        }

        public async Task<ServiceResponse<ChapterAnalyticsResponse>> GetChapterAnalyticsAsync(ChapterAnalyticsRequest request)
        {
            return await _conceptwisePracticeRepository.GetChapterAnalyticsAsync(request);
        }

        public async Task<ServiceResponse<ChapterTimeReportResponse>> GetChapterTimeReportAsync(ChapterAnalyticsRequest request)
        {
            return await _conceptwisePracticeRepository.GetChapterTimeReportAsync(request);
        }

        public async Task<ServiceResponse<QuestionAnalyticsResponseDTO>> GetQuestionAnalyticsAsync(int studentId, int questionId, int setId)
        {
            return await _conceptwisePracticeRepository.GetQuestionAnalyticsAsync(studentId, questionId, setId);
        }
        public async Task<ServiceResponse<QuestionsSetResponse>> GetQuestionsAsync(GetQuestionsList request)
        {
            return await _conceptwisePracticeRepository.GetQuestionsAsync(request);
        }
        public async Task<ServiceResponse<PracticePerformanceStatsDto>> GetStudentPracticeStatsAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            return await _conceptwisePracticeRepository.GetStudentPracticeStatsAsync(studentId, setId, indexTypeId, contentId);
        }
        public async Task<ServiceResponse<StudentTimeAnalysisDto>> GetStudentTimeAnalysisAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            return await _conceptwisePracticeRepository.GetStudentTimeAnalysisAsync(studentId, setId, indexTypeId, contentId);
        }
        public async Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            return await _conceptwisePracticeRepository.GetSyllabusContentDetails(request);
        }

        public async Task<ServiceResponse<List<ChapterTreeResponse>>> GetSyllabusContentDetailsWebView(SyllabusDetailsRequestWebView request)
        {
            return await _conceptwisePracticeRepository.GetSyllabusContentDetailsWebView(request);
        }

        public async Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId)
        {
            return await _conceptwisePracticeRepository.GetSyllabusSubjects(RegistrationId);
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
