using Course_API.DTOs.Requests;
using Course_API.Models;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/Course/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentMasterServices _contentMasterServices;

        public ContentController(IContentMasterServices contentMasterServices)
        {
            _contentMasterServices = contentMasterServices;
        }
        [HttpPost("GetAllContentMaster")]
        public async Task<IActionResult> GetAllContent(GetAllContentListRequest request)
        {
            try
            {
                var data = await _contentMasterServices.GetContentList(request);
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

        [HttpGet("GetContentById/{id}")]
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

        [HttpPost("AddUpdateContent")]
        public async Task<IActionResult> AddUpdateContentMaster([FromBody] ContentMaster request)
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
        [HttpPost("GetContentIndexList")]
        public async Task<IActionResult> GetAllContentIndexList(ContentIndexRequestDTO request)
        {
            try
            {
                var data = await _contentMasterServices.GetAllContentIndexList(request);
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
