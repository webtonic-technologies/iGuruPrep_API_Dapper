using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;
using System.Threading.Tasks;
using static StudentApp_API.Repository.Implementations.ScholarshipRepository;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/Scholarship")]
    [ApiController]
    public class ScholarshipController : ControllerBase
    {
        private readonly IScholarshipService _scholarshipService;

        public ScholarshipController(IScholarshipService scholarshipService)
        {
            _scholarshipService = scholarshipService;
        }

        [HttpPost("AssignScholarship")]
        public async Task<IActionResult> AssignScholarship([FromBody] AssignScholarshipRequest request)
        {
            var response = await _scholarshipService.AssignScholarshipAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetQuestionsByStudentScholarship")]
        public async Task<IActionResult> GetQuestionsByStudentScholarship(GetScholarshipQuestionRequest request)
        {
            var response = await _scholarshipService.ViewKeyByStudentScholarship(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentDiscount")]
        public async Task<IActionResult> GetStudentDiscountAsync(int studentId, int scholarshipTestId)
        {
            var response = await _scholarshipService.GetStudentDiscountAsync(studentId, scholarshipTestId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetScholarshipTest")]
        public async Task<IActionResult> GetScholarshipTest([FromBody] GetScholarshipTestRequest request)
        {
            var response = await _scholarshipService.GetScholarshipTestAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateQuestionNavigation")]
        public async Task<IActionResult> UpdateQuestionNavigation([FromBody] UpdateQuestionNavigationRequest request)
        {
            var response = await _scholarshipService.UpdateQuestionNavigationAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        //[HttpGet("GetQuestionsBySectionSettings/{scholarshipTestId}")]
        //public async Task<IActionResult> GetQuestionsBySectionSettings(int scholarshipTestId)
        //{
        //    var response = await _scholarshipService.GetQuestionsBySectionSettings(scholarshipTestId);
        //    if (response.Success)
        //    {
        //        return Ok(response);
        //    }

        //    return BadRequest(response);
        //}
        [HttpGet("GetScholarshipSubjectQuestionCount/{scholarshipTestId}")]
        public async Task<IActionResult> GetScholarshipSubjectQuestionCount(int scholarshipTestId)
        {
            var response = await _scholarshipService.GetScholarshipSubjectQuestionCount(scholarshipTestId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetScholarshipTestByRegistrationId/{registrationId}")]
        public async Task<IActionResult> GetScholarshipTestByRegistrationId(int registrationId)
        {
            var response = await _scholarshipService.GetScholarshipTestByRegistrationId(registrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetQuestionsBySectionSettings")]
        public async Task<IActionResult> GetQuestionsBySectionSettings(GetScholarshipQuestionRequest request)
        {
            var response = await _scholarshipService.GetQuestionsBySectionSettings(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("SubmitAnswer")]
        public async Task<IActionResult> SubmitAnswer(List<AnswerSubmissionRequest> request)
        {
            var response = await _scholarshipService.SubmitAnswer(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("Save")]
        public async Task<IActionResult> MarkQuestionAsSave(ScholarshipQuestionSaveRequest request)
        {
            var response = await _scholarshipService.MarkScholarshipQuestionAsSave(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetQuestionTypesByScholarshipId/{scholarshipId}")]
        public async Task<IActionResult> GetQuestionTypesByScholarshipId(int scholarshipId)
        {
            var response = await _scholarshipService.GetQuestionTypesByScholarshipId(scholarshipId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("AddReview/{scholarshipId}")]
        public async Task<IActionResult> AddReviewAsync(int scholarshipId, int studentId, int questionId)
        {
            var response = await _scholarshipService.AddReviewAsync(scholarshipId, studentId, questionId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetSubjectWiseTimeSpentReport/{scholarshipId}")]
        public async Task<IActionResult> GetSubjectWiseTimeSpentReportAsync(int studentId, int scholarshipId, int subjectId)
        {
            var response = await _scholarshipService.GetSubjectWiseTimeSpentReportAsync(studentId, scholarshipId, subjectId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetTimeSpentReport/{scholarshipId}")]
        public async Task<IActionResult> GetTimeSpentReportAsync(int studentId, int scholarshipId)
        {
            var response = await _scholarshipService.GetTimeSpentReportAsync(studentId, scholarshipId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpGet("GetSubjectWiseMarksCalculation/{scholarshipId}")]
        public async Task<IActionResult> GetSubjectWiseMarksCalculationAsync(int studentId, int scholarshipId, int subjectId)
        {
            var response = await _scholarshipService.GetSubjectWiseMarksCalculationAsync(studentId, scholarshipId, subjectId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpGet("GetMarksCalculation/{scholarshipId}")]
        public async Task<IActionResult> GetMarksCalculationAsync(int studentId, int scholarshipId)
        {
            var response = await _scholarshipService.GetMarksCalculationAsync(studentId, scholarshipId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpGet("GetSubjectWiseScholarshipAnalytics/{scholarshipId}")]
        public async Task<IActionResult> GetSubjectWiseScholarshipAnalyticsAsync(int studentId, int scholarshipId, int subjectId)
        {
            var response = await _scholarshipService.GetSubjectWiseScholarshipAnalyticsAsync(studentId, scholarshipId, subjectId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpGet("GetScholarshipAnalytics/{scholarshipId}")]
        public async Task<IActionResult> GetScholarshipAnalyticsAsync(int studentId, int scholarshipId)
        {
            var response = await _scholarshipService.GetScholarshipAnalyticsAsync(studentId, scholarshipId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
