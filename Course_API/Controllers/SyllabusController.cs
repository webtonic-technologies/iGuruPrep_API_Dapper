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
        [HttpGet("GetSyllabusDetailById/{syllabusId}/{subjectId}")]
        public async Task<IActionResult> GetSyllabusDetailsById(int syllabusId, int subjectId)
        {
            try
            {
                var data = await _syllabusServices.GetSyllabusDetailsById(syllabusId, subjectId);
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
        [HttpPost("GetSyllabusList")]
        public async Task<IActionResult> GetSyllabusList([FromBody] GetAllSyllabusList request)
        {
            try
            {
                var data = await _syllabusServices.GetSyllabusList(request);
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
        [HttpGet("GetAllContentIndexList/{SubjectId}")]
        public async Task<IActionResult> GetAllContentIndexList(int SubjectId)
        {
            try
            {
                var data = await _syllabusServices.GetAllContentIndexList(SubjectId);
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
        [HttpGet("DownloadExcelFile/{SyllabusId}")]
        public async Task<IActionResult> DownloadExcelFile(int SyllabusId)
        {
            var response = await _syllabusServices.DownloadExcelFile(SyllabusId);
            if (response.Success)
            {
                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SyllabysDetails.xlsx");
            }
            return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadSyllabusDetails(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var response = await _syllabusServices.UploadSyllabusDetails(file);
            if (response.Success)
            {
                return Ok(response.Message);
            }
            return StatusCode(response.StatusCode, response.Message);
        }
    }
}
