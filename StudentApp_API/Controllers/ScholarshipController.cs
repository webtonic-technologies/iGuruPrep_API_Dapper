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
        [HttpGet("GetQuestionsBySectionSettings/{scholarshipTestId}/{studentId}")]
        public async Task<IActionResult> GetQuestionsBySectionSettings(int scholarshipTestId, int studentId)
        {
            var response = await _scholarshipService.GetQuestionsBySectionSettings(scholarshipTestId, studentId);
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
    }
}
