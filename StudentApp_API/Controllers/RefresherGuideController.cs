using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/RefresherGuide")]
    [ApiController]
    public class RefresherGuideController : ControllerBase
    {
        private readonly IRefresherGuideServices _refresherGuideServices;

        public RefresherGuideController(IRefresherGuideServices refresherGuideServices) // Inject the class course service
        {
            _refresherGuideServices = refresherGuideServices;
           
        }
        [HttpPost("GetSyllabusSubjects")]
        public async Task<IActionResult> GetSyllabusSubjects(RefresherGuideRequest request)
        {
            var response = await _refresherGuideServices.GetSyllabusSubjects(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("ShareQuestion")]
        public async Task<IActionResult> ShareQuestionAsync(int studentId, int questionId)
        {
            var response = await _refresherGuideServices.ShareQuestionAsync(studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        //[HttpPost("GetSyllabusContent")]
        //public async Task<IActionResult> GetSyllabusContent(GetContentRequest request)
        //{
        //    var response = await _refresherGuideServices.GetSyllabusContent(request);
        //    if (response.Success)
        //    {
        //        return Ok(response);
        //    }

        //    return BadRequest(response);
        //}
        [HttpPost("GetQuestionsByCriteria")]
        public async Task<IActionResult> GetQuestionsByCriteria(GetQuestionRequest request)
        {
            var response = await _refresherGuideServices.GetQuestionsByCriteria(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("Save")]
        public async Task<IActionResult> MarkQuestionAsSave(SaveQuestionRefresherGuidwRequest request)
        {
            var response = await _refresherGuideServices.MarkQuestionAsSave(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkQuestionAsRead(SaveQuestionRefresherGuidwRequest request)
        {
            var response = await _refresherGuideServices.MarkQuestionAsRead(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetSyllabusContentDetails")]
        public async Task<IActionResult> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            var response = await _refresherGuideServices.GetSyllabusContentDetails(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("question-types/{subjectId}")]
        public async Task<IActionResult> GetDistinctQuestionTypes(int subjectId)
        {
            var result = await _refresherGuideServices.GetDistinctQuestionTypes(subjectId);

            if (result.Success)
            {
                return Ok(result.Data); // Return 200 OK with the data (distinct Question Types)
            }
            else
            {
                return NotFound(result.Message); // Return 404 if no data found
            }
        }
    }
}
