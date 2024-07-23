using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Repository.Interfaces
{
    public interface ITestSeriesRepository
    {
        Task<ServiceResponse<int>> AddUpdateTestSeries(TestSeriesDTO request);
        Task<ServiceResponse<TestSeriesResponseDTO>> GetTestSeriesById(int TestSeriesId);
        Task<ServiceResponse<string>> TestSeriesContentIndexMapping(List<TestSeriesContentIndex> request, int TestSeriesId);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<string>> TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId);
        Task<ServiceResponse<string>> TestSeriesQuestionSectionMapping(List<TestSeriesQuestionSection> request, int TestSeriesId);
        Task<ServiceResponse<string>> TestSeriesInstructionsMapping(List<TestSeriesInstructions> request, int TestSeriesId);
    }
}
