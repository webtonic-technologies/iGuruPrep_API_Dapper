using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IConceptwisePracticeServices
    {
        Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId);
        Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request);
        Task<ServiceResponse<QuestionsSetResponse>> GetQuestionsAsync(GetQuestionsList request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request);
        Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request);
        Task<ServiceResponse<QuestionAnalyticsResponseDTO>> GetQuestionAnalyticsAsync(int studentId, int questionId, int setId);
        Task<ServiceResponse<PracticePerformanceStatsDto>> GetStudentPracticeStatsAsync(int studentId, int setId, int indexTypeId, int contentId);
        Task<ServiceResponse<StudentTimeAnalysisDto>> GetStudentTimeAnalysisAsync(int studentId, int setId, int indexTypeId, int contentId);
        Task<ServiceResponse<ChapterAccuracyReportResponse>> GetChapterAccuracyReportAsync(ChapterAnalyticsRequest request);
        Task<ServiceResponse<ChapterAnalyticsResponse>> GetChapterAnalyticsAsync(ChapterAnalyticsRequest request);
        Task<ServiceResponse<ChapterTimeReportResponse>> GetChapterTimeReportAsync(ChapterAnalyticsRequest request);
        Task<ServiceResponse<List<ChapterTreeResponse>>> GetSyllabusContentDetailsWebView(SyllabusDetailsRequestWebView request);
    }
}
