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
        [HttpPost("GetOnlineQuizoosByRegistrationId")]
        public async Task<IActionResult> GetOnlineQuizoosByRegistrationIdAsync(QuizooListFilters request)
        {
            try
            {
                var data = await _quizooServices.GetOnlineQuizoosByRegistrationIdAsync(request);
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
        [HttpPost("GetChapters")]
        public async Task<IActionResult> GetChaptersAsync(int registrationId, List<int> subjectIds)
        {
            try
            {
                var data = await _quizooServices.GetChaptersAsync(registrationId, subjectIds);
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
        [HttpPost("GetQuizoosByRegistrationId")]
        public async Task<IActionResult> GetQuizoosByRegistrationIdAsync(QuizooListFilters request)
        {
            try
            {
                var data = await _quizooServices.GetQuizoosByRegistrationIdAsync(request);
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
        [HttpPost("GetInvitedQuizoosByRegistrationId")]
        public async Task<IActionResult> GetInvitedQuizoosByRegistrationId(QuizooListFilters request)
        {
            try
            {
                var data = await _quizooServices.GetInvitedQuizoosByRegistrationId(request);
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
        [HttpPost("ShareQuizoo")]
        public async Task<IActionResult> ShareQuizooAsync(int studentId, int quizooId)
        {
            try
            {
                var data = await _quizooServices.ShareQuizooAsync(studentId, quizooId);
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
        [HttpPost("QuizooStart")]
        public async Task<IActionResult> ValidateQuizStartAsync(int quizooId, int studentId)
        {
            try
            {
                var data = await _quizooServices.ValidateQuizStartAsync(quizooId, studentId);
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
        [HttpPost("CheckAndDismissQuizoo")]
        public async Task<IActionResult> CheckAndDismissQuizAsync(int quizooId)
        {
            try
            {
                var data = await _quizooServices.CheckAndDismissQuizAsync(quizooId);
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
        [HttpGet("GetParticipants/{quizooId}/{studentId}")]
        public async Task<IActionResult> GetParticipantsAsync(int quizooId, int studentId)
        {
            try
            {
                var data = await _quizooServices.GetParticipantsAsync(quizooId, studentId);
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
        [HttpPost("SetForceExit/{QuizooID}/{StudentID}")]
        public async Task<IActionResult> SetForceExitAsync(int QuizooID, int StudentID)
        {
            try
            {
                var data = await _quizooServices.SetForceExitAsync(QuizooID, StudentID);
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