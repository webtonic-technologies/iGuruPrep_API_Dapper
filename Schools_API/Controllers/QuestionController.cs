using Microsoft.AspNetCore.Mvc;
using Schools_API.Models;
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
        public async Task<IActionResult> AddQuestion([FromForm] QuestionDTO request)
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
        [HttpPatch("UpdateFile")]
        public async Task<IActionResult> UpdateQuestionImageFile([FromForm] QuestionImageDTO request)
        {
            if (request.QuestionImage == null)
            {
                return BadRequest("The File field is required");
            }
            try
            {
                var data = await _questionServices.UpdateQuestionImageFile(request);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
