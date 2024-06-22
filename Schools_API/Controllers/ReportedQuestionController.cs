using Microsoft.AspNetCore.Mvc;
using Schools_API.DTOs.Requests;
using Schools_API.Services.Interfaces;

namespace Schools_API.Controllers
{
    [Route("iGuru/Schools/[controller]")]
    [ApiController]
    public class ReportedQuestionController : ControllerBase
    {
        private readonly IReportedQuestionsServices _reportedQuestionsServices;

        public ReportedQuestionController(IReportedQuestionsServices reportedQuestionsServices)
        {
            _reportedQuestionsServices = reportedQuestionsServices;
        }
        [HttpPost("UpdateQuestionQuery")]
        public async Task<IActionResult> UpdateQueryForReportedQuestion([FromBody] ReportedQuestionQueryRequest request)
        {
            try
            {

                if (request == null)
                {
                    return BadRequest(" data is null.");
                }

                var data = await _reportedQuestionsServices.UpdateQueryForReportedQuestion(request);

                if (data == null)
                {
                    return StatusCode(500, "A problem happened while handling your request.");
                }

                return Ok(data);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
        [HttpPost("GetAllReportedQuestions")]
        public async Task<IActionResult> GetAllReportedQuestionsList(ReportedQuestionRequest request)
        {
            var data = await _reportedQuestionsServices.GetListOfReportedQuestions(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpGet("GetReportedQuestionById/{QueryCode}")]
        public async Task<IActionResult> GetReportedQuestionById(int QueryCode)
        {
            var data = await _reportedQuestionsServices.GetReportedQuestionById(QueryCode);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
    }
}
