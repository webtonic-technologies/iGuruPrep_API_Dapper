using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/BoardPapers")]
    [ApiController]
    public class BoardPapersController : ControllerBase
    {
        private readonly IBoardPapersServices _boardPapersServices;

        public BoardPapersController(IBoardPapersServices boardPapersServices) // Inject the class course service
        {
            _boardPapersServices = boardPapersServices;

        }
        [HttpGet("GetAllTestSeriesSubjects/{RegistrationId}")]
        public async Task<IActionResult> GetAllTestSeriesSubjects(int RegistrationId)
        {
            var response = await _boardPapersServices.GetAllTestSeriesSubjects(RegistrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetTestSeriesBySubjectId")]
        public async Task<IActionResult> GetTestSeriesBySubjectId(GetTestseriesSubjects request)
        {
            var response = await _boardPapersServices.GetTestSeriesBySubjectId(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetTestSeriesQuestions")]
        public async Task<IActionResult> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        {
            var response = await _boardPapersServices.GetTestSeriesDescriptiveQuestions(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("Save")]
        public async Task<IActionResult> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            var response = await _boardPapersServices.MarkQuestionAsSave(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkQuestionAsRead(SaveQuestionRequest request)
        {
            var response = await _boardPapersServices.MarkQuestionAsRead(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetQuestionTypesByTestSeries/{testSeriesId}")]
        public async Task<IActionResult> GetQuestionTypesByTestSeries(int testSeriesId)
        {
            var result = await _boardPapersServices.GetQuestionTypesByTestSeriesIdAsync(testSeriesId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetTestSeriesPercentageBySubject/{RegistrationId}")]
        public async Task<IActionResult> GetTestSeriesPercentageBySubject(int RegistrationId)
        {
            var result = await _boardPapersServices.GetTestSeriesPercentageBySubject(RegistrationId);

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