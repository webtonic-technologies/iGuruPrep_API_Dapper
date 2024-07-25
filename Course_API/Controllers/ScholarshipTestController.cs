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
        public async Task<IActionResult> ScholarshipContentIndexMapping(List<ScholarshipContentIndex> request, int ScholarshipTestId)
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
        public async Task<IActionResult> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipTestId)
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
        public async Task<IActionResult> ScholarshipInstructionsMapping(List<ScholarshipTestInstructions>? request, int ScholarshipTestId)
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
    }
}
