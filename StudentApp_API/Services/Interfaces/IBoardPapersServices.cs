using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IBoardPapersServices
    {
        Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId);
        Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(GetTestseriesSubjects request);
        Task<ServiceResponse<List<TestSeriesQuestionResponse>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request);
    }
}
