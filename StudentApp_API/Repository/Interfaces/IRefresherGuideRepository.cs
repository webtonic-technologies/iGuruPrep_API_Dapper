using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IRefresherGuideRepository
    {
        Task<ServiceResponse<RefresherGuideSubjects>> GetSyllabusSubjects(RefresherGuideRequest request);
       // Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContent(GetContentRequest request);
        Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRefresherGuidwRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRefresherGuidwRequest request);
        Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetDistinctQuestionTypes(int subjectId);
        Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId);
    }
}
