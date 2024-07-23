using Microsoft.AspNetCore.Mvc;
using Schools_API.DTOs.Requests;
using Schools_API.Services.Interfaces;

namespace Schools_API.Controllers
{
    [Route("iGuru/Schools/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionServices _questionServices;

        public QuestionController(IQuestionServices questionServices)
        {
            _questionServices = questionServices;
        }
        [HttpPost("AddUpdateQuestion")]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionDTO request)
        {
            try
            {

                if (request == null)
                {
                    return BadRequest(" data is null.");
                }

                var data = await _questionServices.AddUpdateQuestion(request);

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
        [HttpPost("GetAllQuestions")]
        public async Task<IActionResult> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            var data = await _questionServices.GetAllQuestionsList(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("GetAllApprovedQuestions")]
        public async Task<IActionResult> GetAllApprovedQuestions(GetAllQuestionListRequest request)
        {
            var data = await _questionServices.GetApprovedQuestionsList(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("GetAllRejectedQuestions")]
        public async Task<IActionResult> GetAllRejectedQuestions(GetAllQuestionListRequest request)
        {
            var data = await _questionServices.GetRejectedQuestionsList(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpGet("GetQuestionById/{QuestionCode}")]
        public async Task<IActionResult> GetQuestionById(string QuestionCode)
        {
            var data = await _questionServices.GetQuestionByCode(QuestionCode);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("QuestionComparison")]
        public async Task<IActionResult> QuestionComparison(QuestionCompareRequest newQuestion)
        {
            var data = await _questionServices.CompareQuestionAsync(newQuestion);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("RejectQuestion")]
        public async Task<IActionResult> RejectQuestion(QuestionRejectionRequestDTO request)
        {
            var data = await _questionServices.RejectQuestion(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("ApproveQuestion")]
        public async Task<IActionResult> ApproveQuestion(QuestionApprovalRequestDTO request)
        {
            var data = await _questionServices.ApproveQuestion(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("QuestionProfiler")]
        public async Task<IActionResult> AssignQuestionProfiler(QuestionProfilerRequest request)
        {
            var data = await _questionServices.AssignQuestionToProfiler(request);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpGet("GetQuestionProfilerById/{QuestionCode}")]
        public async Task<IActionResult> GetQuestionProfilerDetails(string QuestionCode)
        {
            var data = await _questionServices.GetQuestionProfilerDetails(QuestionCode);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
        [HttpPost("QuestionComparison/{QuestionCode}")]
        public async Task<IActionResult> CompareQuestionVersions(string QuestionCode)
        {
            try
            {

                if (QuestionCode == null)
                {
                    return BadRequest(" data is null.");
                }

                var data = await _questionServices.CompareQuestionVersions(QuestionCode);

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
        [HttpGet("GetAssignedQuestionsList/{EmployeeId}")]
        public async Task<IActionResult> GetAssignedQuestionsList(int EmployeeId)
        {
            var data = await _questionServices.GetAssignedQuestionsList(EmployeeId);

            if (data == null)
            {
                return NotFound("No data found.");
            }

            return Ok(data);
        }
    }
}
