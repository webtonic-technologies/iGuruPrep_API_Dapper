using Course_API.DTOs;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentMasterServices _contentMasterServices;

        public ContentController(IContentMasterServices contentMasterServices)
        {
            _contentMasterServices = contentMasterServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllContentMasters()
        {
            try
            {
                var data = await _contentMasterServices.GetContentList();
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

        [HttpGet("Content/{id}")]
        public async Task<IActionResult> GetContentMasterById(int id)
        {
            try
            {
                var data = await _contentMasterServices.GetContentById(id);
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

        [HttpPost]
        public async Task<IActionResult> AddUpdateContentMaster([FromForm] ContentMasterDTO request)
        {
            try
            {
                var data = await _contentMasterServices.AddUpdateContent(request);
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
        [HttpGet("GetFile/{id}")]
        public async Task<IActionResult> GetContentFileById(int id)
        {
            try
            {
                var file = await _contentMasterServices.GetContentFileById(id);
                return File(file.Data, "video/mp4");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetFilePathUrl/{id}")]
        public async Task<IActionResult> GetContentFilePathUrlById(int id)
        {
            try
            {
                var file = await _contentMasterServices.GetContentFilePathUrlById(id);
                return File(file.Data, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
