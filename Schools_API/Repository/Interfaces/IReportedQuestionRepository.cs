using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Repository.Interfaces
{
    public interface IReportedQuestionRepository
    {
        Task<ServiceResponse<string>> UpdateQueryForReportedQuestion(ReportedQuestionQueryRequest request);
        Task<ServiceResponse<List<ReportedQuestionResponse>>> GetListOfReportedQuestions(ReportedQuestionRequest request);
        Task<ServiceResponse<ReportedQuestionResponse>> GetReportedQuestionById(int QueryCode);
        Task<ServiceResponse<string>> AddUpdateReportedQuestion(ReportedQuestionRequestDTO request);
        Task<ServiceResponse<string>> ChangeRQStatus(RQStatusRequest request);
    }
}
