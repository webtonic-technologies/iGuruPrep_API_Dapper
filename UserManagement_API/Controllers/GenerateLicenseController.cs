using Microsoft.AspNetCore.Mvc;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Controllers
{
    [Route("iGuru/UserManagement/[controller]")]
    [ApiController]
    public class GenerateLicenseController : ControllerBase
    {
        private readonly IGenerateLicenseServices _generateLicenseServices;

        public GenerateLicenseController(IGenerateLicenseServices generateLicenseServices)
        {
            _generateLicenseServices = generateLicenseServices;
        }

        [HttpPost("AddUpdate")]
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
        [HttpGet("GetLicenseById/{GenerateLicenseID}")]
        public async Task<IActionResult> GetGenerateLicenseById(int GenerateLicenseID)
        {
            try
            {
                var data = await _generateLicenseServices.GetGenerateLicenseById(GenerateLicenseID);
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
        [HttpPost("GetAllLicenses")]
        public async Task<IActionResult> GetGenerateLicenseList(GetAllLicensesListRequest request)
        {
            try
            {
                var data = await _generateLicenseServices.GetGenerateLicenseList(request);
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
        [HttpGet("GetValidityList")]
        public async Task<IActionResult> GetValidity()
        {
            try
            {
                var data = await _generateLicenseServices.GetValidityList();
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
