using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IRefresherGuideServices
    {
        Task<ServiceResponse<List<RefresherGuideSubjectsResposne>>> GetSyllabusSubjects(RefresherGuideRequest request);
        Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContent(GetContentRequest request);
        Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request);
        Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request);
    }
}
