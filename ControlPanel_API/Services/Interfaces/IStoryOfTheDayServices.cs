﻿using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IStoryOfTheDayServices
    {
        Task<ServiceResponse<string>> AddNewStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO);
        Task<ServiceResponse<string>> UpdateStoryOfTheDay(UpdateStoryOfTheDayDTO storyOfTheDayDTO);
        Task<ServiceResponse<IEnumerable<StoryOfTheDayDTO>>> GetAllStoryOfTheDay();
        Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id);
        Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id);
        Task<ServiceResponse<string>> UpdateStoryOfTheDayFile(StoryOfTheDayIdAndFileDTO storyOfTheDayDTO);
        Task<ServiceResponse<byte[]>> GetStoryOfTheDayFileById(int id);
    }
}