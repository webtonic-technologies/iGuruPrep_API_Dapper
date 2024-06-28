using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
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
        public async Task<ServiceResponse<string>> AddUpdateStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                return await _storyOfTheDayRepository.AddUpdateStoryOfTheDay(storyOfTheDayDTO);
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

        public async Task<ServiceResponse<List<StoryOfTheDayResponseDTO>>> GetAllStoryOfTheDay(SOTDListDTO request)
        {
            try
            {
                return await _storyOfTheDayRepository.GetAllStoryOfTheDay(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StoryOfTheDayResponseDTO>>(false, ex.Message, [], 500);
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

        public async Task<ServiceResponse<StoryOfTheDayResponseDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                return await _storyOfTheDayRepository.GetStoryOfTheDayById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StoryOfTheDayResponseDTO>(false, ex.Message, new StoryOfTheDayResponseDTO(), 500);
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
    }
}
