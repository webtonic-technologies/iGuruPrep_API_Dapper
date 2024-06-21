using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Services.Interfaces
{
    public interface ITimeTablePreparationServices
    {
        Task<ServiceResponse<string>> AddUpdateTimeTable(TimeTablePreparationRequest request);
        Task<ServiceResponse<List<TimeTablePreparationResponseDTO>>> GetAllTimeTableList(TimeTableListRequestDTO request);
        Task<ServiceResponse<TimeTablePreparationResponseDTO>> GetTimeTableById(int PreparationTimeTableId);
    }
}
