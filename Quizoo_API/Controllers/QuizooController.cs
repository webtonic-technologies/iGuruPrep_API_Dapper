using Microsoft.AspNetCore.Mvc;
using Quizoo_API.DTOs.Request;
using Quizoo_API.Services.Interfaces;

namespace Quizoo_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuizooController : ControllerBase
    {
        private readonly IQuizooServices _quizooServices;

        public QuizooController(IQuizooServices quizooServices)
        {
            _quizooServices = quizooServices;
        }
        [HttpGet("GetSubjectsAsync/{registrationId}")]
        public async Task<IActionResult> GetSubjectsAsync(int registrationId)
        {
            try
            {
                var data = await _quizooServices.GetSubjectsAsync(registrationId);
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
        [HttpGet("GetChaptersAsync/{registrationId}/{subjectId}")]
        public async Task<IActionResult> GetChaptersAsync(int registrationId, int subjectId)
        {
            try
            {
                var data = await _quizooServices.GetChaptersAsync(registrationId, subjectId);
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
        [HttpPost("InsertOrUpdateQuizooAsync")]
        public async Task<IActionResult> InsertOrUpdateQuizooAsync(QuizooDTO quizoo)
        {
            try
            {
                var data = await _quizooServices.InsertOrUpdateQuizooAsync(quizoo);
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
        [HttpPost("UpdateQuizooSyllabusAsync/{quizooId}")]
        public async Task<IActionResult> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList)
        {
            try
            {
                var data = await _quizooServices.UpdateQuizooSyllabusAsync(quizooId, syllabusList);
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
