using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/ConceptwisePractice")]
    [ApiController]
    public class ConceptwisePracticeController : ControllerBase
    {
        private readonly IConceptwisePracticeServices _conceptwisePracticeServices;

        public ConceptwisePracticeController(IConceptwisePracticeServices conceptwisePracticeServices) // Inject the class course service
        {
            _conceptwisePracticeServices = conceptwisePracticeServices;

        }
        [HttpGet("GetSyllabusSubjects/{RegistrationId}")]
        public async Task<IActionResult> GetSyllabusSubjects(int RegistrationId)
        {
            var response = await _conceptwisePracticeServices.GetSyllabusSubjects(RegistrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetSyllabusContentDetails")]
        public async Task<IActionResult> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            var response = await _conceptwisePracticeServices.GetSyllabusContentDetails(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetQuestionsAsync")]
        public async Task<IActionResult> GetQuestionsAsync(GetQuestionsList request)
        {
            var response = await _conceptwisePracticeServices.GetQuestionsAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("Save")]
        public async Task<IActionResult> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request)
        {
            var response = await _conceptwisePracticeServices.MarkQuestionAsSave(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("SubmitAnswerAsync")]
        public async Task<IActionResult> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            var response = await _conceptwisePracticeServices.SubmitAnswerAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}