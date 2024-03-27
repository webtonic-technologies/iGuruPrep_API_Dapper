using Config_API.Services.Interfaces;
using iGuruPrep.Models;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassServices _classService;

        public ClassController(IClassServices classService)
        {
            _classService = classService;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateClass(Class request)
        {
            try
            {
                var data = await _classService.AddUpdateClass(request);
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
        [HttpGet("GetAllClasses")]
        public async Task<IActionResult> GetAllClassesList()
        {
            try
            {
                var data = await _classService.GetAllClasses();
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
        [HttpGet("GetClass/{ClassId}")]
        public async Task<IActionResult> GetClassById(int ClassId)
        {
            try
            {
                var data = await _classService.GetClassById(ClassId);
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
        [HttpPut("Status/{ClassId}")]
        public async Task<IActionResult> StatusActiveInactive(int ClassId)
        {
            try
            {
                var data = await _classService.StatusActiveInactive(ClassId);
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
