using Config_API.DTOs.Requests;
using Config_API.Models;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class QuestionTypeController : ControllerBase
    {
        private readonly IQuestionTypeService _questionTypeService;

        public QuestionTypeController(IQuestionTypeService questionTypeService)
        {
            _questionTypeService = questionTypeService;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateQuestionType(Questiontype request)
        {
            try
            {
                var data = await _questionTypeService.AddUpdateQuestionType(request);
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
        [HttpPost("GetAllQuestionTypeList")]
        public async Task<IActionResult> GetAllQuestionTypeList(GetAllQuestionTypeRequest request)
        {
            try
            {
                var data = await _questionTypeService.GetQuestionTypeList(request);
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
        [HttpGet("GetQuestionType/{Id}")]
        public async Task<IActionResult> GetBoardById(int Id)
        {
            try
            {
                var data = await _questionTypeService.GetQuestionTypeByID(Id);
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
        [HttpPut("Status/{Id}")]
        public async Task<IActionResult> StatusActiveInactive(int Id)
        {
            try
            {
                var data = await _questionTypeService.StatusActiveInactive(Id);
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
        [HttpGet("GetAllOptionTypeList")]
        public async Task<IActionResult> OptionTypesList()
        {
            try
            {
                var data = await _questionTypeService.OptionTypesList();
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
        [HttpGet("GetAllNoOfOptionsList")]
        public async Task<IActionResult> NoOfOptionsList()
        {
            try
            {
                var data = await _questionTypeService.NoOfOptionsList();
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
        [HttpGet("GetAllQuestionTypeListMasters")]
        public async Task<IActionResult> GetAllQuestionTypeListMasters()
        {
            try
            {
                var data = await _questionTypeService.GetQuestionTypeListMasters();
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
