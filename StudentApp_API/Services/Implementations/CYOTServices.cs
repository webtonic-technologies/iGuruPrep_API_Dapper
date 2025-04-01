using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class CYOTServices : ICYOTServices
    {
        private readonly ICYOTRepository _cYOTRepository;

        public CYOTServices(ICYOTRepository cYOTRepository)
        {
            _cYOTRepository = cYOTRepository;
        }

        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(GetChaptersRequestCYOT request)
        {
            return await _cYOTRepository.GetChaptersAsync(request);
        }

        public async Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            return await _cYOTRepository.GetCYOTAnalyticsAsync(studentId, cyotId);
        }

        public async Task<ServiceResponse<CYOTDTO>> GetCYOTByIdAsync(int cyotId)
        {
            return await _cYOTRepository.GetCYOTByIdAsync(cyotId);
        }

        public async Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            return await _cYOTRepository.GetCYOTQestionReportBySubjectAsync(cyotId, studentId, subjectId);
        }

        public async Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportAsync(int studentId, int cyotId)
        {
            return await _cYOTRepository.GetCYOTQestionReportAsync(studentId, cyotId);
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetCYOTQuestions(GetCYOTQuestionsRequest request)
        {
            return await _cYOTRepository.GetCYOTQuestions(request);
        }

        public async Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(GetCYOTQuestionsRequest request)
        {
            return await _cYOTRepository.GetCYOTQuestionsWithOptionsAsync(request);
        }

        public async Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            return await _cYOTRepository.GetCYOTTimeAnalyticsAsync(studentId, cyotId);
        }

        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            return await _cYOTRepository.GetSubjectsAsync(registrationId);
        }

        public async Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot)
        {
            return await _cYOTRepository.InsertOrUpdateCYOTAsync(cyot);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionCYOTRequest request)
        {
            return await _cYOTRepository.MarkQuestionAsSave(request);
        }

        public async Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int CYOTId)
        {
            return await _cYOTRepository.ShareQuestionAsync(studentId, questionId, CYOTId);
        }

        public async Task<ServiceResponse<string>> UpdateQuestionNavigationAsync(CYOTQuestionNavigationRequest request)
        {
            return await _cYOTRepository.UpdateQuestionNavigationAsync(request);
        }

        public async Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList)
        {
            return await _cYOTRepository.UpdateCYOTSyllabusAsync(cyotId, syllabusList);
        }

        public async Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int cyotId, int studentId, int questionId, bool isAnswered)
        {
            return await _cYOTRepository.UpdateQuestionStatusAsync(cyotId, studentId, questionId, isAnswered);
        }

        public async Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            return await _cYOTRepository.GetCYOTAnalyticsBySubjectAsync(cyotId, studentId, subjectId);
        }

        public async Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            return await _cYOTRepository.GetCYOTTimeAnalyticsBySubjectAsync(cyotId, studentId, subjectId);
        }
    }
}