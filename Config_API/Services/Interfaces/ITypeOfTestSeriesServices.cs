using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface ITypeOfTestSeriesServices
    {
        Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeries(GetAllTestSeriesTypesRequest request);
        Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeriesMasters();
        Task<ServiceResponse<TypeOfTestSeries>> GetTestSeriesById(int id);
        Task<ServiceResponse<string>> AddUpdateTestSeries(TypeOfTestSeries request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
