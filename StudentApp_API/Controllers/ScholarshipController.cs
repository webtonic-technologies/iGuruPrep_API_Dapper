using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;
using System.Threading.Tasks;

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
    }
}
