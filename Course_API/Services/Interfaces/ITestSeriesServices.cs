using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Services.Interfaces
{
    public interface ITestSeriesServices
    {
        Task<ServiceResponse<string>> AddUpdateTestSeries(TestSeriesDTO request);
    }
}
