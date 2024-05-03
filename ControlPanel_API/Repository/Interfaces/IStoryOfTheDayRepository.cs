using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IStoryOfTheDayRepository
    {

        Task<ServiceResponse<string>> AddNewStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO);
        Task<ServiceResponse<string>> UpdateStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO);
        Task<ServiceResponse<List<StoryOfTheDayDTO>>> GetAllStoryOfTheDay(SOTDListDTO request);
        Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id);
        Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<List<EventType>>> GetEventtypeList();
    }
}
