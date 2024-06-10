using Microsoft.AspNetCore.Mvc;
using Schools_API.DTOs;
using Schools_API.Models;
using Schools_API.Services.Implementations;
using Schools_API.Services.Interfaces;

namespace Schools_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionServices _questionServices;

        public QuestionController(IQuestionServices questionServices)
        {
            _questionServices = questionServices;
        }
        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionDTO request)
        {
            try
            {

                if (request == null)
                {
                    return BadRequest("Project data is null.");
                }

                var data = await _questionServices.AddQuestion(request);

                if (data == null)
                {
                    return StatusCode(500, "A problem happened while handling your request.");
                }

                return Ok(data);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
        [HttpPost("GetAllQuestions")]
        public async Task<IActionResult> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            var projects = await _questionServices.GetAllQuestionsList(request);

            if (projects == null)
            {
                return NotFound("No projects found.");
            }

            return Ok(projects);
        }
        [HttpGet("GetQuestionById/{id}")]
        public async Task<IActionResult> GetQuestionById(int id)
        {
            var projects = await _questionServices.GetQuestionById(id);

            if (projects == null)
            {
                return NotFound("No projects found.");
            }

            return Ok(projects);
        }
    }
}
