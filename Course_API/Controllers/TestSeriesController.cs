using Course_API.DTOs.Requests;
using Course_API.Models;
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
        [HttpPost("TestSeriesContentIndexMapping")]
        public async Task<IActionResult> TestSeriesContentIndexMapping(List<TestSeriesContentIndex> request, int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesContentIndexMapping(request,TestSeriesId);
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
        [HttpPost("GetQuestionsList")]
        public async Task<IActionResult> GetQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                var data = await _testSeriesServices.GetQuestionsList(request);
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
        [HttpPost("TestSeriesQuestionsMapping")]
        public async Task<IActionResult> TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesQuestionsMapping(request, TestSeriesId, sectionId);
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
        [HttpPost("TestSeriesQuestionSectionMapping")]
        public async Task<IActionResult> TestSeriesQuestionSectionMapping(List<TestSeriesQuestionSection> request, int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesQuestionSectionMapping(request, TestSeriesId);
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
        [HttpPost("TestSeriesInstructionsMapping")]
        public async Task<IActionResult> TestSeriesInstructionsMapping(List<TestSeriesInstructions> request, int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesInstructionsMapping(request, TestSeriesId);
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
        [HttpPost("GetSyllabusDetailsBySubject")]
        public async Task<IActionResult> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                var data = await _testSeriesServices.GetSyllabusDetailsBySubject(request);
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
        [HttpPost("GetTestSeriesList")]
        public async Task<IActionResult> GetTestSeriesList(TestSeriesListRequest request)
        {
            try
            {
                var data = await _testSeriesServices.GetTestSeriesList(request);
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
        [HttpPost("GetAutoGeneratedQuestionList")]
        public async Task<IActionResult> GetAutoGeneratedQuestionList(QuestionListRequest request)
        {
            try
            {
                var data = await _testSeriesServices.GetAutoGeneratedQuestionList(request);
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
