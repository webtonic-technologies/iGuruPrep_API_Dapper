using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Repository.Interfaces;
using Schools_API.Services.Interfaces;

namespace Schools_API.Services.Implementations
{
    public class ReportedQuestionsServices : IReportedQuestionsServices
    {
        private readonly IReportedQuestionRepository _reportedQuestionRepository;

        public ReportedQuestionsServices(IReportedQuestionRepository reportedQuestionRepository)
        {
            _reportedQuestionRepository = reportedQuestionRepository;
        }

        public async Task<ServiceResponse<string>> AddUpdateReportedQuestion(ReportedQuestionRequestDTO request)
        {
            try
            {
                return await _reportedQuestionRepository.AddUpdateReportedQuestion(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ChangeRQStatus(RQStatusRequest request)
        {
            try
            {
                return await _reportedQuestionRepository.ChangeRQStatus(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<ReportedQuestionResponse>>> GetListOfReportedQuestions(ReportedQuestionRequest request)
        {
            try
            {
                return await _reportedQuestionRepository.GetListOfReportedQuestions(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ReportedQuestionResponse>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<ReportedQuestionResponse>> GetReportedQuestionById(int QueryCode)
        {
            try
            {
                return await _reportedQuestionRepository.GetReportedQuestionById(QueryCode);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ReportedQuestionResponse>(false, ex.Message, new ReportedQuestionResponse(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateQueryForReportedQuestion(ReportedQuestionQueryRequest request)
        {

            try
            {
                return await _reportedQuestionRepository.UpdateQueryForReportedQuestion(request);
            }
            catch (Exception ex)
            {
               return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
