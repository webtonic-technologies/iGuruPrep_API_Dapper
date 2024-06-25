using Course_API.DTOs.Requests;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/Course/[controller]")]
    [ApiController]
    public class TestSeriesController : ControllerBase
    {
        private readonly ITestSeriesServices _testSeriesServices;

        public TestSeriesController(ITestSeriesServices testSeriesServices)
        {
            _testSeriesServices = testSeriesServices;
        }

        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateTestSeries([FromBody] TestSeriesDTO request)
        {
            try
            {
                var data = await _testSeriesServices.AddUpdateTestSeries(request);
                if (data != null)
                {
                    return Ok(data);
                }
                else
                {
                    return BadRequest("Bad Request");
                }
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpPost("GetTestSeriesById/{TestSeriesId}")]
        public async Task<IActionResult> GetTestSeriesById(int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.GetTestSeriesById(TestSeriesId);
                if (data != null)
                {
                    return Ok(data);
                }
                else
                {
                    return BadRequest("Bad Request");
                }
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
    }
}
