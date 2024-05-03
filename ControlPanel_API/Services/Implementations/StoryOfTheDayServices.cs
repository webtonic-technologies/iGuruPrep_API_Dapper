using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class StoryOfTheDayServices : IStoryOfTheDayServices
    {
        private readonly IStoryOfTheDayRepository _storyOfTheDayRepository;

        public StoryOfTheDayServices(IStoryOfTheDayRepository storyOfTheDayRepository)
        {
            _storyOfTheDayRepository = storyOfTheDayRepository;
        }
        public async Task<ServiceResponse<string>> AddNewStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                return await _storyOfTheDayRepository.AddNewStoryOfTheDay(storyOfTheDayDTO);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id)
        {
            try
            {
                return await _storyOfTheDayRepository.DeleteStoryOfTheDay(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<List<StoryOfTheDayDTO>>> GetAllStoryOfTheDay(SOTDListDTO request)
        {
            try
            {
                return await _storyOfTheDayRepository.GetAllStoryOfTheDay(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StoryOfTheDayDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<EventType>>> GetEventtypeList()
        {
            try
            {
                return await _storyOfTheDayRepository.GetEventtypeList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<EventType>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                return await _storyOfTheDayRepository.GetStoryOfTheDayById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StoryOfTheDayDTO>(false, ex.Message, new StoryOfTheDayDTO(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _storyOfTheDayRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                return await _storyOfTheDayRepository.UpdateStoryOfTheDay(storyOfTheDayDTO);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
