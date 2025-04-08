using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
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
        [HttpGet("GetQuestionAnalytics/{studentId}/{questionId}/{setId}")]
        public async Task<IActionResult> GetQuestionAnalyticsAsync(int studentId, int questionId, int setId)
        {
            var response = await _conceptwisePracticeServices.GetQuestionAnalyticsAsync(studentId, questionId, setId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentPracticeStats/{studentId}/{setId}/{indexTypeId}/{contentId}")]
        public async Task<IActionResult> GetStudentPracticeStatsAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            var response = await _conceptwisePracticeServices.GetStudentPracticeStatsAsync(studentId, setId, indexTypeId, contentId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentTimeAnalysis/{studentId}/{setId}/{indexTypeId}/{contentId}")]
        public async Task<IActionResult> GetStudentTimeAnalysisAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            var response = await _conceptwisePracticeServices.GetStudentTimeAnalysisAsync(studentId, setId, indexTypeId, contentId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetChapterAccuracyReport")]
        public async Task<IActionResult> GetChapterAccuracyReportAsync(ChapterAccuracyReportRequest request)
        {
            var response = await _conceptwisePracticeServices.GetChapterAccuracyReportAsync(request);
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
    }
}