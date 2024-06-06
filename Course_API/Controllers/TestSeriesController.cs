using Course_API.DTOs;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class TestSeriesController : ControllerBase
    {
        private readonly ITestSeriesServices _testSeriesServices;

        public TestSeriesController(ITestSeriesServices testSeriesServices)
        {
            _testSeriesServices = testSeriesServices;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TestSeriesDTO request)
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
    }
}
