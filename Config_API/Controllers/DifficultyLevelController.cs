using Config_API.DTOs.Requests;
using Config_API.Models;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class DifficultyLevelController : ControllerBase
    {
        private readonly IDifficultyLevelServices _questionLevelService;

        public DifficultyLevelController(IDifficultyLevelServices questionLevelServices)
        {
            _questionLevelService = questionLevelServices;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateDifficultyLevel(DifficultyLevel request)
        {
            try
            {
                var data = await _questionLevelService.AddUpdateQuestionLevel(request);
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
        [HttpPost("GetAllDifficultyLevels")]
        public async Task<IActionResult> GetAllDifficultyLevelsList(GetAllDifficultyLevelRequest request)
        {
            try
            {
                var data = await _questionLevelService.GetAllQuestionLevel(request);
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
        [HttpGet("GetDifficultyLevel/{DifficultyLevelId}")]
        public async Task<IActionResult> GetDifficultyLevelById(int DifficultyLevelId)
        {
            try
            {
                var data = await _questionLevelService.GetQuestionLevelById(DifficultyLevelId);
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
        [HttpPut("Status/{DifficultyLevelId}")]
        public async Task<IActionResult> StatusActiveInactive(int DifficultyLevelId)
        {
            try
            {
                var data = await _questionLevelService.StatusActiveInactive(DifficultyLevelId);
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
        [HttpGet("GetAllDifficultyLevelsMasters")]
        public async Task<IActionResult> GetAllDifficultyLevelsListMasters()
        {
            try
            {
                var data = await _questionLevelService.GetAllQuestionLevelMasters();
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
    }
}
