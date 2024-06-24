using Course_API.DTOs.Requests;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/Course/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusServices _syllabusServices;

        public SyllabusController(ISyllabusServices syllabusServices)
        {
            _syllabusServices = syllabusServices;
        }

        [HttpPost("AddUpdateSyllabus")]
        public async Task<IActionResult> AddUpdateSyllabus([FromBody] SyllabusDTO request)
        {
            try
            {
                var data = await _syllabusServices.AddUpdateSyllabus(request);
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

        [HttpPost("AddUpdateSyllabusDetails")]
        public async Task<IActionResult> AddUpdateSyllabusDetails([FromBody] SyllabusDetailsDTO request)
        {
            try
            {
                var data = await _syllabusServices.AddUpdateSyllabusDetails(request);
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
        [HttpGet("GetSyllabusDetailById/{syllabusId}")]
        public async Task<IActionResult> GetSyllabusDetailsById(int syllabusId)
        {
            try
            {
                var data = await _syllabusServices.GetSyllabusDetailsById(syllabusId);
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
        [HttpGet("GetSyllabusById/{syllabusId}")]
        public async Task<IActionResult> GetSyllabusById(int syllabusId)
        {
            try
            {
                var data = await _syllabusServices.GetSyllabusById(syllabusId);
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
        [HttpPut("UpdateContentIndexName")]
        public async Task<IActionResult> UpdateContentIndexName(UpdateContentIndexNameDTO request)
        {
            try
            {
                var data = await _syllabusServices.UpdateContentIndexName(request);
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
