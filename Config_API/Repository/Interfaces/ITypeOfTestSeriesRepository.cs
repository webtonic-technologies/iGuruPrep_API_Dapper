using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface ITypeOfTestSeriesRepository
    {
        Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeries(GetAllTestSeriesTypesRequest request);
        Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeriesMasters();
        Task<ServiceResponse<TypeOfTestSeries>> GetTestSeriesById(int id);
        Task<ServiceResponse<string>> AddUpdateTestSeries(TypeOfTestSeries request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
