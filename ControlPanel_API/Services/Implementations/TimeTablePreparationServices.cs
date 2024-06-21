using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class TimeTablePreparationServices : ITimeTablePreparationServices
    {

        private readonly ITimeTablePreparationRepository _timeTablePreparationRepository;

        public TimeTablePreparationServices(ITimeTablePreparationRepository timeTablePreparationRepository)
        {
            _timeTablePreparationRepository = timeTablePreparationRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateTimeTable(TimeTablePreparationRequest request)
        {
            try
            {
                return await _timeTablePreparationRepository.AddUpdateTimeTable(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<TimeTablePreparationResponseDTO>>> GetAllTimeTableList(TimeTableListRequestDTO request)
        {
            try
            {
                return await _timeTablePreparationRepository.GetAllTimeTableList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TimeTablePreparationResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<TimeTablePreparationResponseDTO>> GetTimeTableById(int PreparationTimeTableId)
        {
            try
            {
                return await _timeTablePreparationRepository.GetTimeTableById(PreparationTimeTableId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TimeTablePreparationResponseDTO>(false, ex.Message, new TimeTablePreparationResponseDTO(), 500);
            }
        }
    }
}
