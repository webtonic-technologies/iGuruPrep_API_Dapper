using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class TestSeriesServices : ITestSeriesServices
    {

        private readonly ITestSeriesRepository _testSeriesRepository;

        public TestSeriesServices(ITestSeriesRepository testSeriesRepository)
        {
            _testSeriesRepository = testSeriesRepository;
        }
        public async Task<ServiceResponse<int>> AddUpdateTestSeries(TestSeriesDTO request)
        {
            try
            {
                return await _testSeriesRepository.AddUpdateTestSeries(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }

        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                return await _testSeriesRepository.GetQuestionsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                return await _testSeriesRepository.GetSyllabusDetailsBySubject(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<TestSeriesResponseDTO>> GetTestSeriesById(int TestSeriesId)
        {
            try
            {
                return await _testSeriesRepository.GetTestSeriesById(TestSeriesId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestSeriesResponseDTO>(false, ex.Message, new TestSeriesResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<TestSeriesResponseDTO>>> GetTestSeriesList(TestSeriesListRequest request)
        {
            try
            {
                return await _testSeriesRepository.GetTestSeriesList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<string>> TestSeriesContentIndexMapping(List<TestSeriesContentIndex> request, int TestSeriesId)
        {
            try
            {
                return await _testSeriesRepository.TestSeriesContentIndexMapping(request, TestSeriesId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> TestSeriesInstructionsMapping(List<TestSeriesInstructions> request, int TestSeriesId)
        {
            try
            {
                return await _testSeriesRepository.TestSeriesInstructionsMapping(request, TestSeriesId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> TestSeriesQuestionSectionMapping(List<TestSeriesQuestionSection> request, int TestSeriesId)
        {
            try
            {
                return await _testSeriesRepository.TestSeriesQuestionSectionMapping(request, TestSeriesId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId)
        {
            try
            {
                return await _testSeriesRepository.TestSeriesQuestionsMapping(request, TestSeriesId, sectionId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
