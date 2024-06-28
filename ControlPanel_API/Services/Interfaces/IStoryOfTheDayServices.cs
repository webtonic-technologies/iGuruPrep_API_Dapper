using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IStoryOfTheDayServices
    {
        Task<ServiceResponse<string>> AddUpdateStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO);
        Task<ServiceResponse<List<StoryOfTheDayResponseDTO>>> GetAllStoryOfTheDay(SOTDListDTO request);
        Task<ServiceResponse<StoryOfTheDayResponseDTO>> GetStoryOfTheDayById(int id);
        Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<List<EventType>>> GetEventtypeList();
    }
}
