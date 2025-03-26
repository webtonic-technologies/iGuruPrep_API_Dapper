using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface ICYOTServices
    {
        Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId);
        Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, List<int> subjectIds);
        Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot);
        Task<ServiceResponse<CYOTDTO>> GetCYOTByIdAsync(int cyotId);
        Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetCYOTQuestions(GetCYOTQuestionsRequest request);
        Task<ServiceResponse<string>> UpdateQuestionNavigationAsync(CYOTQuestionNavigationRequest request);
        Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(GetCYOTQuestionsRequest request);
        Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId);
        Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request);
        Task<ServiceResponse<bool>> UpsertCYOTParticipantsAsync(List<CYOTParticipantRequest> requests);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionCYOTRequest request);
        Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int CYOTId);
        Task<ServiceResponse<string>> DeleteCYOT(int CYOTId);
        Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportAsync(int studentId, int cyotId);
        Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId);
        Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId);
        Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportBySubjectAsync(int cyotId, int studentId, int subjectId);
        Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId);
        Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId); 
        Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int cyotId, int studentId, int questionId, bool isAnswered);
    }
}
