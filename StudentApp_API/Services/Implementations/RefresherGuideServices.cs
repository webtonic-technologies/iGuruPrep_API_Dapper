using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class RefresherGuideServices : IRefresherGuideServices
    {
        private readonly IRefresherGuideRepository _refresherGuideRepository;

        public RefresherGuideServices(IRefresherGuideRepository refresherGuideRepository)
        {
            _refresherGuideRepository = refresherGuideRepository;
        }

        public async Task<ServiceResponse<List<QuestionResponse>>> GetQuestionsByCriteria(GetQuestionRequest request)
        {
            return await _refresherGuideRepository.GetQuestionsByCriteria(request);
        }

        public async Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContent(GetContentRequest request)
        {
            return await _refresherGuideRepository.GetSyllabusContent(request);
        }

        public async Task<ServiceResponse<List<RefresherGuideContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            return await _refresherGuideRepository.GetSyllabusContentDetails(request);
        }

        public async Task<ServiceResponse<List<RefresherGuideSubjectsResposne>>> GetSyllabusSubjects(RefresherGuideRequest request)
        {
            return await _refresherGuideRepository.GetSyllabusSubjects(request);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request)
        {
            return await _refresherGuideRepository.MarkQuestionAsRead(request);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            return await _refresherGuideRepository.MarkQuestionAsSave(request);
        }
    }
}
