using ControlPanel_API.DTOs;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class StoryOfTheDayController : ControllerBase
    {

        private readonly IStoryOfTheDayServices _storyOfTheDayService;

        public StoryOfTheDayController(IStoryOfTheDayServices storyOfTheDayServices)
        {
            _storyOfTheDayService = storyOfTheDayServices;
        }

        [HttpPost("AddStoryofTheDay")]
        public async Task<IActionResult> AddNewStoryOfTheDay([FromBody] StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                var data = await _storyOfTheDayService.AddNewStoryOfTheDay(storyOfTheDayDTO);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateStoryofTheDay")]
        public async Task<IActionResult> UpdateStoryOfTheDay([FromBody] StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                var data = await _storyOfTheDayService.UpdateStoryOfTheDay(storyOfTheDayDTO);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetListofStoryofTheDay")]
        public async Task<IActionResult> GetAllStoryOfTheDay(SOTDListDTO request)
        {
            try
            {
                var storyOfTheDays = await _storyOfTheDayService.GetAllStoryOfTheDay(request);
                return Ok(storyOfTheDays);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetofStoryofTheDayById/{id}")]
        public async Task<IActionResult> GetStoryOfTheDayById(int id)
        {
            try
            {
                var storyOfTheDay = await _storyOfTheDayService.GetStoryOfTheDayById(id);
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteStoryOftheDay/{id}")]
        public async Task<IActionResult> DeleteStoryOfTheDay(int id)
        {
            try
            {
                var data = await _storyOfTheDayService.DeleteStoryOfTheDay(id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Status/{id}")]
        public async Task<IActionResult> StatusActiveInactive(int id)
        {
            try
            {
                var data = await _storyOfTheDayService.StatusActiveInactive(id);
                if (data != null)
                {
                    return Ok(data);

                }
                else
                {
                    return BadRequest("Bad Request");
                }

            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }

        }
        [HttpGet("GetAllEventTypes")]
        public async Task<IActionResult> GetEventTypesList()
        {
            try
            {
                var storyOfTheDay = await _storyOfTheDayService.GetEventtypeList();
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
