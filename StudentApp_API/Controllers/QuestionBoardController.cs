using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionBoardController : ControllerBase
    {
        private readonly IQuizooQuestionBoardServices _quizooQuestionBoardServices;

        public QuestionBoardController(IQuizooQuestionBoardServices quizooQuestionBoardServices)
        {
            _quizooQuestionBoardServices = quizooQuestionBoardServices;
        }
        [HttpGet("GetQuizQuestions/{quizooId}/{registrationId}")]
        public async Task<IActionResult> GetQuizQuestions(int quizooId, int registrationId)
        {
            try
            {
                var data = await _quizooQuestionBoardServices.GetQuizQuestions(quizooId, registrationId);
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
        [HttpPost("SubmitAnswer")]
        public async Task<IActionResult> SubmitAnswerAsync(SubmitAnswerRequest request)
        {
            try
            {
                var data = await _quizooQuestionBoardServices.SubmitAnswerAsync(request);
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
