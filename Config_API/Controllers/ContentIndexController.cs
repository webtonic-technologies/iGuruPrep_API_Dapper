using Config_API.DTOs.Requests;
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
    }
}
