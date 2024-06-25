using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
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
        public async Task<ServiceResponse<string>> AddUpdateTestSeries(TestSeriesDTO request)
        {
            try
            {
                return await _testSeriesRepository.AddUpdateTestSeries(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
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
    }
}
