using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
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
        [HttpPost("GetChapters")]
        public async Task<IActionResult> GetChaptersAsync(GetChaptersRequestCYOT request)
        {
            var response = await _cYOTServices.GetChaptersAsync(request);
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
        [HttpPost("GetCYOTQuestions")]
        public async Task<IActionResult> GetCYOTQuestions(GetCYOTQuestionsRequest request)
        {
            var result = await _cYOTServices.GetCYOTQuestions(request);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("UpdateQuestionNavigation")]
        public async Task<IActionResult> UpdateQuestionNavigationAsync(CYOTQuestionNavigationRequest request)
        {
            var result = await _cYOTServices.UpdateQuestionNavigationAsync(request);

            if (result.Success)
            {
                return Ok(result.Message); // Return 200 OK with the data (distinct Question Types)
            }
            else
            {
                return NotFound(result.Message); // Return 404 if no data found
            }
        }
        [HttpPost("GetCYOTQuestionsWithOptions")]
        public async Task<IActionResult> GetCYOTQuestionsWithOptionsAsync(GetCYOTQuestionsRequest request)
        {
            var response = await _cYOTServices.GetCYOTQuestionsWithOptionsAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
       
        [HttpPost("MarkQuestionAsSave")]
        public async Task<IActionResult> MarkQuestionAsSave(SaveQuestionCYOTRequest request)
        {
            var result = await _cYOTServices.MarkQuestionAsSave(request);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("ShareQuestion")]
        public async Task<IActionResult> ShareQuestionAsync(int studentId, int questionId, int CYOTId)
        {
            var result = await _cYOTServices.ShareQuestionAsync(studentId, questionId, CYOTId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
       
        [HttpGet("GetCYOTQestionReport")]
        public async Task<IActionResult> GetCYOTQestionReportAsync(int studentId, int cyotId)
        {
            var result = await _cYOTServices.GetCYOTQestionReportAsync(studentId, cyotId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTAnalytics")]
        public async Task<IActionResult> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            var result = await _cYOTServices.GetCYOTAnalyticsAsync(studentId, cyotId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTTimeAnalytics")]
        public async Task<IActionResult> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            var result = await _cYOTServices.GetCYOTTimeAnalyticsAsync(studentId, cyotId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTQestionReportBySubject")]
        public async Task<IActionResult> GetCYOTQestionReportBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            var result = await _cYOTServices.GetCYOTQestionReportBySubjectAsync(cyotId, studentId, subjectId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTTimeAnalyticsBySubject")]
        public async Task<IActionResult> GetCYOTTimeAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            var result = await _cYOTServices.GetCYOTTimeAnalyticsBySubjectAsync(cyotId, studentId, subjectId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("UpdateQuestionStatus")]
        public async Task<IActionResult> UpdateQuestionStatusAsync(int cyotId, int studentId, int questionId, bool isAnswered)
        {
            var response = await _cYOTServices.UpdateQuestionStatusAsync(cyotId, studentId, questionId, isAnswered);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTAnalyticsBySubject")]
        public async Task<IActionResult> GetCYOTAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            var result = await _cYOTServices.GetCYOTAnalyticsBySubjectAsync(cyotId, studentId, subjectId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
    }
}