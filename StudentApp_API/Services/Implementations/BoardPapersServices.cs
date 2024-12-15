using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class BoardPapersServices : IBoardPapersServices
    {
        private readonly IBoardPapersRepository _boardPapersRepository;

        public BoardPapersServices(IBoardPapersRepository boardPapersRepository)
        {
            _boardPapersRepository = boardPapersRepository;
        }

        public async Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId)
        {
            return await _boardPapersRepository.GetAllTestSeriesSubjects(RegistrationId);
        }

        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByTestSeriesIdAsync(int testSeriesId)
        {
            return await _boardPapersRepository.GetQuestionTypesByTestSeriesIdAsync(testSeriesId);
        }

        public async Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(GetTestseriesSubjects request)
        {
            return await _boardPapersRepository.GetTestSeriesBySubjectId(request);
        }

        public async Task<ServiceResponse<List<TestSeriesQuestionsList>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        {
            return await _boardPapersRepository.GetTestSeriesDescriptiveQuestions(request);
        }

        public async Task<ServiceResponse<Dictionary<string, object>>> GetTestSeriesPercentageBySubject(int RegistrationId)
        {
            return await _boardPapersRepository.GetTestSeriesPercentageBySubject(RegistrationId);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request)
        {
            return await _boardPapersRepository.MarkQuestionAsRead(request);
        }

        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            return await _boardPapersRepository.MarkQuestionAsSave(request);
        }
    }
}
