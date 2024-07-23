using Config_API.DTOs.Requests;
using Config_API.Models;
using Config_API.Services.Implementations;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class ContentIndexController : ControllerBase
    {
        private readonly IContentIndexServices _contentIndexServices;

        public ContentIndexController(IContentIndexServices contentIndexServices)
        {
            _contentIndexServices = contentIndexServices;
        }
        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateContentIndex(ContentIndexRequest request)
        {
            try
            {
                var data = await _contentIndexServices.AddUpdateContentIndex(request);
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
        [HttpPost("GetAllContentIndex")]
        public async Task<IActionResult> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {
                var data = await _contentIndexServices.GetAllContentIndexList(request);
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
        [HttpGet("GetContentIndexById/{ContentIndexId}")]
        public async Task<IActionResult> GetContentIndexById(int ContentIndexId)
        {
            try
            {
                var data = await _contentIndexServices.GetContentIndexById(ContentIndexId);
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
        [HttpPut("Status/{ContentIndexId}")]
        public async Task<IActionResult> StatusActiveInactive(int ContentIndexId)
        {
            try
            {
                var data = await _contentIndexServices.StatusActiveInactive(ContentIndexId);
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
        [HttpPost("GetAllContentIndexMasters")]
        public async Task<IActionResult> GetAllContentIndexListMasters(ContentIndexMastersDTO request)
        {
            try
            {
                var data = await _contentIndexServices.GetAllContentIndexListMasters(request);
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
        [HttpPost("AddUpdateContentIndexChapter")]
        public async Task<IActionResult> AddUpdateContentIndexChapter(ContentIndexRequestdto request)
        {
            try
            {
                var data = await _contentIndexServices.AddUpdateContentIndexChapter(request);
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
        [HttpPost("AddUpdateContentIndexTopics")]
        public async Task<IActionResult> AddUpdateContentIndexTopics(ContentIndexTopicsdto request)
        {
            try
            {
                var data = await _contentIndexServices.AddUpdateContentIndexTopics(request);
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
        [HttpPost("AddUpdateContentIndexSubTopics")]
        public async Task<IActionResult> AddUpdateContentIndexSubTopics(ContentIndexSubTopic request)
        {
            try
            {
                var data = await _contentIndexServices.AddUpdateContentIndexSubTopics(request);
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
        [HttpGet("download/{subjectId}")]
        public async Task<IActionResult> DownloadContentIndex(int subjectId)
        {
            var response = await _contentIndexServices.DownloadContentIndexBySubjectId(subjectId);
            if (response.Success)
            {
                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ContentIndex.xlsx");
            }
            return StatusCode(response.StatusCode, response.Message);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadContentIndex(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var response = await _contentIndexServices.UploadContentIndex(file);
            if (response.Success)
            {
                return Ok(response.Message);
            }
            return StatusCode(response.StatusCode, response.Message);
        }
    }
}
