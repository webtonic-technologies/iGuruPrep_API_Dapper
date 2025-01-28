using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IConceptwisePracticeServices
    {
        Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId);
        Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsAsync(GetQuestionsList request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request);
        Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request);
    }
}
