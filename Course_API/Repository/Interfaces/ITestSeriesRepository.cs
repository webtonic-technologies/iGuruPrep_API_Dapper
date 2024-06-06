using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Repository.Interfaces
{
    public interface ITestSeriesRepository
    {
        Task<ServiceResponse<string>> AddUpdateTestSeries(TestSeriesDTO request);
    }
}
