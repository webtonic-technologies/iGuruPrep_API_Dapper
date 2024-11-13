using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IBoardPapersRepository
    {
        Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId);
        Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(int subjectId);
        Task<ServiceResponse<List<TestSeriesQuestionResponse>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request);
    }
}
