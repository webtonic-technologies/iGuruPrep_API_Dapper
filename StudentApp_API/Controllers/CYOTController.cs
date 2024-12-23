using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/CYOT")]
    [ApiController]
    public class CYOTController : ControllerBase
    {
        private readonly ICYOTServices _cYOTServices;

        public CYOTController(ICYOTServices cYOTServices) // Inject the class course service
        {
            _cYOTServices = cYOTServices;

        }
        [HttpGet("GetSubjects/{registrationId}")]
        public async Task<IActionResult> GetSubjectsAsync(int registrationId)
        {
            var response = await _cYOTServices.GetSubjectsAsync(registrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetChapters/{registrationId}/{subjectId}")]
        public async Task<IActionResult> GetChaptersAsync(int registrationId, int subjectId)
        {
            var response = await _cYOTServices.GetChaptersAsync(registrationId, subjectId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("InsertOrUpdateCYOT")]
        public async Task<IActionResult> InsertOrUpdateCYOTAsync(CYOTDTO cyot)
        {
            var response = await _cYOTServices.InsertOrUpdateCYOTAsync(cyot);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("UpdateCYOTSyllabus/{cyotId}")]
        public async Task<IActionResult> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList)
        {
            var response = await _cYOTServices.UpdateCYOTSyllabusAsync(cyotId, syllabusList);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTById/{cyotId}")]
        public async Task<IActionResult> GetCYOTByIdAsync(int cyotId)
        {
            var response = await _cYOTServices.GetCYOTByIdAsync(cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTQuestions/{cyotId}/{registrationId}")]
        public async Task<IActionResult> GetCYOTQuestions(int cyotId, int registrationId)
        {
            var result = await _cYOTServices.GetCYOTQuestions(cyotId, registrationId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("SubmitCYOTAnswerAsync")]
        public async Task<IActionResult> SubmitCYOTAnswerAsync(SubmitAnswerRequest request)
        {
            var result = await _cYOTServices.SubmitCYOTAnswerAsync(request);

            if (result.Success)
            {
                return Ok(result.Data); // Return 200 OK with the data (distinct Question Types)
            }
            else
            {
                return NotFound(result.Message); // Return 404 if no data found
            }
        }
        [HttpGet("GetCYOTQuestionsWithOptions/{cyotId}")]
        public async Task<IActionResult> GetCYOTQuestionsWithOptionsAsync(int cyotId)
        {
            var response = await _cYOTServices.GetCYOTQuestionsWithOptionsAsync(cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}