using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

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
        [HttpGet("GetSubjects/{registrationId}")]
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
        [HttpGet("GetOnlineQuizoosByRegistrationId/{registrationId}")]
        public async Task<IActionResult> GetOnlineQuizoosByRegistrationIdAsync(int registrationId)
        {
            try
            {
                var data = await _quizooServices.GetOnlineQuizoosByRegistrationIdAsync(registrationId);
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
        [HttpGet("GetQuizooByIdAsync/{quizooId}")]
        public async Task<IActionResult> GetQuizooByIdAsync(int quizooId)
        {
            try
            {
                var data = await _quizooServices.GetQuizooByIdAsync(quizooId);
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
        [HttpGet("GetChapters/{registrationId}/{subjectId}")]
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
        [HttpPost("InsertOrUpdateQuizoo")]
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
        [HttpPost("UpdateQuizooSyllabus/{quizooId}")]
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
        [HttpGet("GetQuizoosByRegistrationId/{registrationId}")]
        public async Task<IActionResult> GetQuizoosByRegistrationIdAsync(int registrationId)
        {
            try
            {
                var data = await _quizooServices.GetQuizoosByRegistrationIdAsync(registrationId);
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
        [HttpGet("GetInvitedQuizoosByRegistrationId/{registrationId}")]
        public async Task<IActionResult> GetInvitedQuizoosByRegistrationId(int registrationId)
        {
            try
            {
                var data = await _quizooServices.GetInvitedQuizoosByRegistrationId(registrationId);
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
