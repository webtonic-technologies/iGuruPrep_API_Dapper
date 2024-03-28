using ControlPanel_API.DTOs;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Implementations;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        [HttpPost]
        public async Task<IActionResult> AddNewStoryOfTheDay([FromForm] StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                await _storyOfTheDayService.AddNewStoryOfTheDay(storyOfTheDayDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStoryOfTheDay([FromForm] UpdateStoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                await _storyOfTheDayService.UpdateStoryOfTheDay(storyOfTheDayDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStoryOfTheDay()
        {
            try
            {
                var storyOfTheDays = await _storyOfTheDayService.GetAllStoryOfTheDay();
                return Ok(storyOfTheDays);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoryOfTheDay(int id)
        {
            try
            {
                await _storyOfTheDayService.DeleteStoryOfTheDay(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateFile")]
        public async Task<IActionResult> UpdateStoryOfTheDayFile([FromForm] StoryOfTheDayIdAndFileDTO storyOfTheDayDTO)
        {
            if (storyOfTheDayDTO.UploadImage == null)
            {
                return BadRequest("The File field is required");
            }

            try
            {
                await _storyOfTheDayService.UpdateStoryOfTheDayFile(storyOfTheDayDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetFile/{id}")]
        public async Task<IActionResult> GetStoryOfTheDayFileById(int id)
        {
            try
            {
                var file = await _storyOfTheDayService.GetStoryOfTheDayFileById(id);
                return File(file.Data, "image/*");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
