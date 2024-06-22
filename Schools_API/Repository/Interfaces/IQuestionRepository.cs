using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Repository.Interfaces
{
    public interface IQuestionRepository
    {
        Task<ServiceResponse<QuestionResponseDTO>> GetQuestionById(int questionId);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<string>> AddUpdateQuestion(QuestionDTO request);
        Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion);
        Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request);
        Task<ServiceResponse<string>> ApproveQuestion(QuestionApprovalRequestDTO request);
        Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request);
        Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(int QuestionId);
    }
}
