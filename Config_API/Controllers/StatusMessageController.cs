using Config_API.Models;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class StatusMessageController : ControllerBase
    {
        private readonly IStatusMessageServices _statusMessageService;

        public StatusMessageController(IStatusMessageServices statusMessageServices)
        {
            _statusMessageService = statusMessageServices;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateStatusMessage(StatusMessages request)
        {
            try
            {
                var data = await _statusMessageService.AddUpdateStatusMessage(request);
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
        [HttpGet("GetStatusMessage/{StatusId}")]
        public async Task<IActionResult> GetStatusMessageById(int StatusId)
        {
            try
            {
                var data = await _statusMessageService.GetStatusMessageById(StatusId);
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
        [HttpGet]
        public async Task<IActionResult> GetStatusMessageList()
        {
            try
            {
                var data = await _statusMessageService.GetStatusMessageList();
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
