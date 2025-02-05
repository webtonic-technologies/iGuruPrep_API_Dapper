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
        [HttpPost("AddUpdateDuplicateTestSeries/{TestSeriesId}")]
        public async Task<IActionResult> AddUpdateDuplicateTestSeries(int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.AddUpdateDuplicateTestSeries(TestSeriesId);
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
        public async Task<IActionResult> TestSeriesContentIndexMapping(ContentIndexRequest request, int TestSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesContentIndexMapping(request, TestSeriesId);
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
        public async Task<IActionResult> TestSeriesQuestionsMapping(List<TestSeriesQuestionsMapping> request, int TestSeriesId, int sectionId)
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
        public async Task<IActionResult> TestSeriesQuestionSectionMapping(List<QuestionSection> request, int TestSeriesId)
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
        public async Task<IActionResult> TestSeriesInstructionsMapping(TestSeriesInstructions request, int TestSeriesId)
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
        [HttpGet("sections/{testSeriesId}")]
        public async Task<IActionResult> GetSectionsByTestSeriesId(int testSeriesId)
        {
            var response = await _testSeriesServices.GetSectionsByTestSeriesId(testSeriesId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        [HttpGet("questiontypes/{sectionId}")]
        public async Task<IActionResult> GetQuestionTypesBySectionId(int sectionId)
        {
            var response = await _testSeriesServices.GetQuestionTypesBySectionId(sectionId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        [HttpGet("difficultylevels/{sectionId}")]
        public async Task<IActionResult> GetDifficultyLevelsBySectionId(int sectionId)
        {
            var response = await _testSeriesServices.GetDifficultyLevelsBySectionId(sectionId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        [HttpGet("contentindexhierarchy/{testSeriesId}")]
        public async Task<IActionResult> GetTestSeriesContentIndexHierarchy(int testSeriesId)
        {
            var response = await _testSeriesServices.GetTestSeriesContentIndexHierarchy(testSeriesId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        [HttpPost("assign-test-series")]
        public async Task<IActionResult> AssignTestSeries([FromBody] TestseriesProfilerRequest request)
        {
            var response = await _testSeriesServices.AssignTestSeries(request);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }
        [HttpPut("UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion(QuestionDTO request)
        {
            try
            {
                var data = await _testSeriesServices.UpdateQuestion(request);
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
        [HttpPost("DownloadExcelFile")]
        public async Task<IActionResult> DownloadExcelFile(DownExcelRequest request)
        {
            var response = await _testSeriesServices.GenerateExcelFile(request);
            if (response.Success)
            {
                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "QuestionDetails.xlsx");
            }
            return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPost("upload/{testSeriesId}/{EmployeeId}")]
        public async Task<IActionResult> UploadQuestionsFromExcel(IFormFile file, int testSeriesId, int EmployeeId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var response = await _testSeriesServices.UploadQuestionsFromExcel(file, testSeriesId, EmployeeId);
            if (response.Success)
            {
                return Ok(response.Message);
            }
            return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPost("TestSeriesRejectedQuestionRemarks")]
        public async Task<IActionResult> TestSeriesRejectedQuestionRemarks(RejectedQuestionRemark request)
        {
            try
            {
                var data = await _testSeriesServices.TestSeriesRejectedQuestionRemarks(request);
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
        [HttpPost("ApproveRejectedQuestion/{testSeriesId}/{QuestionId}")]
        public async Task<IActionResult> ApproveRejectedQuestion(int testSeriesId, int QuestionId)
        {
            var response = await _testSeriesServices.ApproveRejectedQuestion(testSeriesId, QuestionId);
            if (response.Success)
            {
                return Ok(response.Message);
            }
            return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPut("AddUpdateTestSeriesDateAndTime")]
        public async Task<IActionResult> AddUpdateTestSeriesDateAndTime(TestSeriesDateAndTimeRequest request)
        {
            try
            {
                var data = await _testSeriesServices.AddUpdateTestSeriesDateAndTime(request);
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
        [HttpGet("GetRepeatedExamQuestions/{testSeriesId}")]
        public async Task<IActionResult> GetRepeatedExamQuestions(int testSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.GetRepeatedExamQuestions(testSeriesId);
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
        [HttpGet("GetSingleExamQuestions/{testSeriesId}")]
        public async Task<IActionResult> GetSingleExamQuestions(int testSeriesId)
        {
            try
            {
                var data = await _testSeriesServices.GetSingleExamQuestions(testSeriesId);
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
        [HttpGet("GetQuestionsByTestSeriesAndDate/{testSeriesId}")]
        public async Task<IActionResult> GetQuestionsByTestSeriesAndDateAsync(int testSeriesId, DateTime examDate)
        {
            var response = await _testSeriesServices.GetQuestionsByTestSeriesAndDateAsync(testSeriesId, examDate);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        [HttpGet("GetRepetitiveExamDates/{testSeriesId}")]
        public async Task<IActionResult> GetRepetitiveExamDates(int testSeriesId)
        {
            var response = await _testSeriesServices.GetRepetitiveExamDates(testSeriesId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
    }
}
