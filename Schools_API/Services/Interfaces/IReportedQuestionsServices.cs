using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Services.Interfaces
{
    public interface IReportedQuestionsServices
    {
        Task<ServiceResponse<string>> UpdateQueryForReportedQuestion(ReportedQuestionQueryRequest request);
        Task<ServiceResponse<List<ReportedQuestionResponse>>> GetListOfReportedQuestions(ReportedQuestionRequest request);
        Task<ServiceResponse<ReportedQuestionResponse>> GetReportedQuestionById(int QueryCode);
    }
}
