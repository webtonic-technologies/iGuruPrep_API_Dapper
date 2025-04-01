using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/OpenChallenges")]
    [ApiController]
    public class OpenChallengesController : ControllerBase
    {
        private readonly IOpenChallengesServices _openChallengesServices;

        public OpenChallengesController(IOpenChallengesServices openChallengesServices) // Inject the class course service
        {
            _openChallengesServices = openChallengesServices;

        }
        [HttpPost("GetOpenChallenges")]
        public async Task<IActionResult> GetOpenChallengesAsync(CYOTListRequest request)
        {
            var response = await _openChallengesServices.GetOpenChallengesAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("StartChallenge")]
        public async Task<IActionResult> StartChallengeAsync(int studentId, int cyotId)
        {
            var response = await _openChallengesServices.StartChallengeAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}