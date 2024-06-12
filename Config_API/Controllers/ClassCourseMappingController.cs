using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class ClassCourseMappingController : ControllerBase
    {
        private readonly IClassCourseMappingServices _classCourseMappingServices;

        public ClassCourseMappingController(IClassCourseMappingServices classCourseMappingServices)
        {
            _classCourseMappingServices = classCourseMappingServices;
        }
        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateClassCourseMapping(ClassCourseMappingDTO request)
        {
            try
            {
                var data = await _classCourseMappingServices.AddUpdateClassCourseMapping(request);
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
        [HttpPost("GetAllClassCourseMappings")]
        public async Task<IActionResult> GetAllClassCourseMappingsList(GetAllClassCourseRequest request)
        {
            try
            {
                var data = await _classCourseMappingServices.GetAllClassCoursesMappings(request);
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
        [HttpGet("GetClassCourseMappingById/{CourseClassMappingID}")]
        public async Task<IActionResult> GetClassCourseMappingById(int CourseClassMappingID)
        {
            try
            {
                var data = await _classCourseMappingServices.GetClassCourseMappingById(CourseClassMappingID);
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
        [HttpPut("Status/{CourseClassMappingID}")]
        public async Task<IActionResult> StatusActiveInactive(int CourseClassMappingID)
        {
            try
            {
                var data = await _classCourseMappingServices.StatusActiveInactive(CourseClassMappingID);
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
