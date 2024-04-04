using Course_API.DTOs;
using Course_API.Models;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusServices _syllabusServices;

        public SyllabusController(ISyllabusServices syllabusServices)
        {
            _syllabusServices = syllabusServices;
        }

        [HttpPost("Syllabus")]
        public async Task<IActionResult> AddUpdateSyllabus([FromBody] Syllabus request)
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

        [HttpPost("SyllabusDetail")]
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
    }
}
