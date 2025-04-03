using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/MyChallenges")]
    [ApiController]
    public class MyChallengesController : ControllerBase
    {
        private readonly IMyChallengesServices _myChallengesServices;

        public MyChallengesController(IMyChallengesServices myChallengesServices) // Inject the class course service
        {
            _myChallengesServices = myChallengesServices;

        }
        [HttpPost("GetCYOTListByStudent")]
        public async Task<IActionResult> GetCYOTListByStudent(CYOTListRequest request)
        {
            var response = await _myChallengesServices.GetCYOTListByStudent(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpDelete("DeleteCYOT/{CYOTId}")]
        public async Task<IActionResult> DeleteCYOT(int CYOTId)
        {
            var response = await _myChallengesServices.DeleteCYOT(CYOTId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("MakeCYOTOpenChallenge")]
        public async Task<IActionResult> MakeCYOTOpenChallenge(int CYOTId, int studentId)
        {
            var response = await _myChallengesServices.MakeCYOTOpenChallenge(CYOTId, studentId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTAnalytics/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            var response = await _myChallengesServices.GetCYOTAnalyticsAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTTimeAnalytics/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            var response = await _myChallengesServices.GetCYOTTimeAnalyticsAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTMarksComparison/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTMarksComparisonAsync(int studentId, int cyotId)
        {
            var result = await _myChallengesServices.GetCYOTMarksComparisonAsync(studentId, cyotId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTSubjectWiseAnalytics/{studentId}/{cyotId}/{subjectId}")]
        public async Task<IActionResult> GetCYOTSubjectWiseAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            var result = await _myChallengesServices.GetCYOTSubjectWiseAnalyticsAsync(studentId, cyotId, subjectId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTSubjectWiseTimeAnalytics/{studentId}/{cyotId}/{subjectId}")]
        public async Task<IActionResult> GetCYOTSubjectWiseTimeAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            var result = await _myChallengesServices.GetCYOTSubjectWiseTimeAnalyticsAsync(studentId, cyotId, subjectId);
            if (result.Success)
                return Ok(result);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetCYOTLeaderboard/{cyotId}/{studentId}")]
        public async Task<IActionResult> GetCYOTLeaderboardAsync(int cyotId, int studentId)
        {
            var response = await _myChallengesServices.GetCYOTLeaderboardAsync(cyotId, studentId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTPercentageComparison/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTPercentageComparisonAsync(int studentId, int cyotId)
        {
            var response = await _myChallengesServices.GetCYOTPercentageComparisonAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTCorrectAnswersComparison/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTCorrectAnswersComparisonAsync(int studentId, int cyotId)
        {
            var response = await _myChallengesServices.GetCYOTCorrectAnswersComparisonAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCYOTIncorrectAnswersComparison/{studentId}/{cyotId}")]
        public async Task<IActionResult> GetCYOTIncorrectAnswersComparisonAsync(int studentId, int cyotId)
        {
            var response = await _myChallengesServices.GetCYOTIncorrectAnswersComparisonAsync(studentId, cyotId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}