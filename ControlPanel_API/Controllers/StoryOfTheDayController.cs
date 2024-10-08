using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Implementations;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class StoryOfTheDayController : ControllerBase
    {

        private readonly IStoryOfTheDayServices _storyOfTheDayService;

        public StoryOfTheDayController(IStoryOfTheDayServices storyOfTheDayServices)
        {
            _storyOfTheDayService = storyOfTheDayServices;
        }

        [HttpPost("AddUpdateStoryofTheDay")]
        public async Task<IActionResult> AddUpdateStoryOfTheDay([FromBody] StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                var data = await _storyOfTheDayService.AddUpdateStoryOfTheDay(storyOfTheDayDTO);
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

        [HttpGet("GetofStoryofTheDayById/{StoryId}")]
        public async Task<IActionResult> GetStoryOfTheDayById(int StoryId)
        {
            try
            {
                var storyOfTheDay = await _storyOfTheDayService.GetStoryOfTheDayById(StoryId);
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Status/{StoryId}")]
        public async Task<IActionResult> StatusActiveInactive(int StoryId)
        {
            try
            {
                var data = await _storyOfTheDayService.StatusActiveInactive(StoryId);
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
        [HttpGet("GetCategoryList")]
        public async Task<IActionResult> GetCategoryList()
        {
            try
            {
                var storyOfTheDay = await _storyOfTheDayService.GetCategoryList();
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[HttpGet("PublishStory")]
        //public async Task<IActionResult> GetStoryOfTheDayByPublishDateAndTime()
        //{
        //    try
        //    {
        //        var magazine = await _storyOfTheDayService.GetStoryOfTheDayByPublishDateAndTime();
        //        return Ok(magazine);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
