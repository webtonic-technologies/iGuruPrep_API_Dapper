using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class TypeOfTestSeriesServices : ITypeOfTestSeriesServices
    {
        private readonly ITypeOfTestSeriesRepository _typeOfTestSeriesRepository;

        public TypeOfTestSeriesServices(ITypeOfTestSeriesRepository typeOfTestSeriesRepository)
        {
            _typeOfTestSeriesRepository = typeOfTestSeriesRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateTestSeries(TypeOfTestSeries request)
        {
            try
            {
                return await _typeOfTestSeriesRepository.AddUpdateTestSeries(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeries(GetAllTestSeriesTypesRequest request)
        {
            try
            {
                return await _typeOfTestSeriesRepository.GetListOfTestSeries(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TypeOfTestSeries>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeriesMasters()
        {
            try
            {
                return await _typeOfTestSeriesRepository.GetListOfTestSeriesMasters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TypeOfTestSeries>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<TypeOfTestSeries>> GetTestSeriesById(int id)
        {
            try
            {
                return await _typeOfTestSeriesRepository.GetTestSeriesById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TypeOfTestSeries>(false, ex.Message, new TypeOfTestSeries(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {

            try
            {
                return await _typeOfTestSeriesRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
