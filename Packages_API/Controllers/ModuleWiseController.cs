using Microsoft.AspNetCore.Mvc;
using Packages_API.DTOs.Response;
using Packages_API.Services.Interfaces;

namespace Packages_API.Controllers
{
    [ApiController]
    [Route("api/ModuleWise")]
    public class ModuleWiseController : ControllerBase
    {
        private readonly IModuleWiseServices _moduleWiseServices;

        public ModuleWiseController(IModuleWiseServices moduleWiseServices)
        {
            _moduleWiseServices = moduleWiseServices;
        }

        [HttpGet("GetModuleWiseConfiguration")]
        public async Task<IActionResult> GetModuleWiseConfiguration()
        {
            var response = await _moduleWiseServices.GetModuleWiseConfiguration();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        [HttpGet("GetModuleNames")]
        public async Task<IActionResult> GetModuleNames()
        {
            var response = await _moduleWiseServices.GetModules();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        [HttpPost("SetModuleWiseConfiguration")]
        public async Task<IActionResult> SetModuleWiseConfiguration([FromBody] List<ModuleWiseConfigDTO> request)
        {
            var response = await _moduleWiseServices.SetModuleWiseConfiguration(request);
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}
