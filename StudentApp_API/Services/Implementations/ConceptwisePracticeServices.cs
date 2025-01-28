using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class ConceptwisePracticeServices : IConceptwisePracticeServices
    {
        private readonly IConceptwisePracticeRepository _conceptwisePracticeRepository;

        public ConceptwisePracticeServices(IConceptwisePracticeRepository conceptwisePracticeRepository)
        {
            _conceptwisePracticeRepository = conceptwisePracticeRepository;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsAsync(GetQuestionsList request)
        {
            return await _conceptwisePracticeRepository.GetQuestionsAsync(request);
        }

        public async Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            return await _conceptwisePracticeRepository.GetSyllabusContentDetails(request);
        }

        public async Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId)
        {
            return await _conceptwisePracticeRepository.GetSyllabusSubjects(RegistrationId);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request)
        {
            return await _conceptwisePracticeRepository.MarkQuestionAsSave(request);
        }

        public async Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            return await _conceptwisePracticeRepository.SubmitAnswerAsync(request);
        }
    }
}
