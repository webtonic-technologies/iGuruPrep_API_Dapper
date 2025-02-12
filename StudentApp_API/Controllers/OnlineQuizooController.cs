using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OnlineQuizooController : ControllerBase
    {
        private readonly IOnlineQuizooServices _onlineQuizooServices;

        public OnlineQuizooController(IOnlineQuizooServices onlineQuizooServices)
        {
            _onlineQuizooServices = onlineQuizooServices;
        }
        [HttpPost("InsertQuizoo")]
        public async Task<IActionResult> InsertQuizooAsync(QuizooDTO quizoo)
        {
            try
            {
                var data = await _onlineQuizooServices.InsertQuizooAsync(quizoo);
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

        [HttpGet("GetQuestionsWithCorrectAnswers/{quizooId}")]
        public async Task<IActionResult> GetQuestionsWithCorrectAnswersAsync(int quizooId)
        {
            try
            {
                var data = await _onlineQuizooServices.GetQuestionsWithCorrectAnswersAsync(quizooId);
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

        [HttpGet("GetStudentRankList/{quizooId}/{userId}")]
        public async Task<IActionResult> GetStudentRankListAsync(int quizooId, int userId)
        {
            try
            {
                var data = await _onlineQuizooServices.GetStudentRankListAsync(quizooId, userId);
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
        [HttpPost("SetForceExit/{qpid}")]
        public async Task<IActionResult> SetForceExitAsync(int qpid)
        {
            try
            {
                var data = await _onlineQuizooServices.SetForceExitAsync(qpid);
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
        [HttpGet("ShareQuestion")]
        public async Task<IActionResult> ShareQuestionAsync(int studentId, int questionId, int QuizooId)
        {
            try
            {
                var data = await _onlineQuizooServices.ShareQuestionAsync(studentId, questionId, QuizooId);
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