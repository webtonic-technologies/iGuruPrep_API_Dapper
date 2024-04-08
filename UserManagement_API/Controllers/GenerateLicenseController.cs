using Microsoft.AspNetCore.Mvc;
using UserManagement_API.DTOs;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class GenerateLicenseController : ControllerBase
    {
        private readonly IGenerateLicenseServices _generateLicenseServices;

        public GenerateLicenseController(IGenerateLicenseServices generateLicenseServices)
        {
            _generateLicenseServices = generateLicenseServices;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateGenerateLicense(GenerateLicenseDTO request)
        {
            try
            {
                var data = await _generateLicenseServices.AddUpdateGenerateLicense(request);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenerateLicenseById(int id)
        {
            try
            {
                var data = await _generateLicenseServices.GetGenerateLicenseById(id);
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
        public async Task<IActionResult> GetGenerateLicenseList()
        {
            try
            {
                var data = await _generateLicenseServices.GetGenerateLicenseList();
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
