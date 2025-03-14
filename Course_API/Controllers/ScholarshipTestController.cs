using Course_API.DTOs.Requests;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/Course/[controller]")]
    [ApiController]
    public class ScholarshipTestController : ControllerBase
    {
        private readonly IScholarshipTestServices _scholarshipTestServices;

        public ScholarshipTestController(IScholarshipTestServices scholarshipTestServices)
        {
            _scholarshipTestServices = scholarshipTestServices;
        }
        [HttpPost("GetAllScholarshipTests")]
        public async Task<IActionResult> GetScholarshipTestList(ScholarshipGetListRequest request)
        {
            try
            {
                var data = await _scholarshipTestServices.GetScholarshipTestList(request);
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
        [HttpGet("GetScholarshipById/{ScholarshipTestId}")]
        public async Task<IActionResult> GetScholarshipTestById(int ScholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.GetScholarshipTestById(ScholarshipTestId);
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
        [HttpPost("AddUpdateScholarship")]
        public async Task<IActionResult> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request)
        {
            try
            {
                var data = await _scholarshipTestServices.AddUpdateScholarshipTest(request);
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
        [HttpPost("ScholarshipContentIndexMapping/{ScholarshipTestId}")]
        public async Task<IActionResult> ScholarshipContentIndexMapping(ContentIndexRequest request, int ScholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.ScholarshipContentIndexMapping(request, ScholarshipTestId);
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
        [HttpPost("ScholarshipQuestionsMapping/{ScholarshipTestId}/{SSTSectionId}")]
        public async Task<IActionResult> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId)
        {
            try
            {
                var data = await _scholarshipTestServices.ScholarshipQuestionsMapping(request, ScholarshipTestId, SSTSectionId);
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
        [HttpPost("ScholarshipQuestionSectionMapping/{ScholarshipTestId}")]
        public async Task<IActionResult> ScholarshipQuestionSectionMapping(List<QuestionSectionScholarship> request, int ScholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.ScholarshipQuestionSectionMapping(request, ScholarshipTestId);
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
        [HttpPost("ScholarshipInstructionsMapping/{ScholarshipTestId}")]
        public async Task<IActionResult> ScholarshipInstructionsMapping(ScholarshipTestInstructions? request, int ScholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.ScholarshipInstructionsMapping(request, ScholarshipTestId);
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
        [HttpPost("ScholarshipDiscountSchemeMapping/{ScholarshipTestId}")]
        public async Task<IActionResult> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.ScholarshipDiscountSchemeMapping(request, ScholarshipTestId);
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
        public async Task<IActionResult> GetSyllabusDetailsBySubject(SyllabusDetailsRequestScholarship request)
        {
            try
            {
                var data = await _scholarshipTestServices.GetSyllabusDetailsBySubject(request);
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
        [HttpGet("GetScholarshipDetails/{scholarshipTestId}")]
        public async Task<IActionResult> GetScholarshipDetails(int scholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.GetScholarshipDetails(scholarshipTestId);
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
        [HttpPut("ToggleScholarshipTestStatus/{scholarshipTestId}")]
        public async Task<IActionResult> ToggleScholarshipTestStatus(int scholarshipTestId)
        {
            try
            {
                var data = await _scholarshipTestServices.ToggleScholarshipTestStatus(scholarshipTestId);
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
        [HttpGet("GetScholarshipQuestionsAsync/{scholarshipTestId}/{studentId}")]
        public async Task<IActionResult> GetScholarshipQuestionsAsync(int scholarshipTestId, int studentId)
        {
            try
            {
                var data = await _scholarshipTestServices.GetScholarshipQuestionsAsync(scholarshipTestId, studentId);
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
        //[HttpPost("ScholarshipQuestions/{scholarshipTestId}")]
        //public async Task<IActionResult> AssignScholarshipQuestionsAsync(int scholarshipTestId)
        //{
        //    try
        //    {
        //        var data = await _scholarshipTestServices.AssignScholarshipQuestionsAsync(scholarshipTestId);
        //        if (data != null)
        //        {
        //            return Ok(data);

        //        }
        //        else
        //        {
        //            return BadRequest("Bad Request");
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return this.BadRequest(e.Message);
        //    }
        //}
    }
}
