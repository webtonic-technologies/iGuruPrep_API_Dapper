using Microsoft.AspNetCore.Mvc;
using UserManagement_API.DTOs;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class GenerateReferenceController : ControllerBase
    {
        private readonly IGenerateReferenceServices _generateReferenceServices;

        public GenerateReferenceController(IGenerateReferenceServices generateReferenceServices)
        {
            _generateReferenceServices = generateReferenceServices;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateGenerateReference(GenerateReferenceDTO request)
        {
            try
            {
                var data = await _generateReferenceServices.AddUpdateGenerateReference(request);
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
        public async Task<IActionResult> GetGenerateReferenceById(int id)
        {
            try
            {
                var data = await _generateReferenceServices.GetGenerateReferenceById(id);
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
        public async Task<IActionResult> GetGenerateReferenceList()
        {
            try
            {
                var data = await _generateReferenceServices.GetGenerateReferenceList();
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
