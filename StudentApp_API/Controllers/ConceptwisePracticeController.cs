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
        [HttpPost("GetQuestions")]
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
        [HttpPost("SubmitAnswer")]
        public async Task<IActionResult> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            var response = await _conceptwisePracticeServices.SubmitAnswerAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentQuestionAccuracy/{studentId}/{questionId}")]
        public async Task<IActionResult> GetStudentQuestionAccuracyAsync(int studentId, int questionId)
        {
            var response = await _conceptwisePracticeServices.GetStudentQuestionAccuracyAsync(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentGroupAccuracyForQuestion/{studentId}/{questionId}")]
        public async Task<IActionResult> GetStudentGroupAccuracyForQuestionAsync(int studentId, int questionId)
        {
            var response = await _conceptwisePracticeServices.GetStudentGroupAccuracyForQuestionAsync(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetAverageTimeSpentByOtherStudents/{studentId}/{questionId}")]
        public async Task<IActionResult> GetAverageTimeSpentByOtherStudents(int studentId, int questionId)
        {
            var response = await _conceptwisePracticeServices.GetAverageTimeSpentByOtherStudents(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetQuestionAttemptStatsForGroup/{studentId}/{questionId}")]
        public async Task<IActionResult> GetQuestionAttemptStatsForGroupAsync(int studentId, int questionId)
        {
            var response = await _conceptwisePracticeServices.GetQuestionAttemptStatsForGroupAsync(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetAverageTimeSpentOnQuestion/{studentId}/{questionId}")]
        public async Task<IActionResult> GetAverageTimeSpentOnQuestion(int studentId, int questionId)
        {
            var response = await _conceptwisePracticeServices.GetAverageTimeSpentOnQuestion(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}