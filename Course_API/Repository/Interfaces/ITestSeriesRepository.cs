using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Repository.Interfaces
{
    public interface ITestSeriesRepository
    {
        Task<ServiceResponse<string>> AddUpdateTestSeries(TestSeriesDTO request);
        Task<ServiceResponse<TestSeriesResponseDTO>> GetTestSeriesById(int TestSeriesId);
    }
}
