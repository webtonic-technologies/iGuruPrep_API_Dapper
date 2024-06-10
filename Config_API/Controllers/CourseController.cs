using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using iGuruPrep.Models;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseServices _courseService;

        public CourseController(ICourseServices courseServices)
        {
            _courseService = courseServices;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateCourse(Course request)
        {
            try
            {
                var data = await _courseService.AddUpdateCourse(request);
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
        [HttpPost("GetAllCourses")]
        public async Task<IActionResult> GetAllCoursesList(GetAllCoursesRequest request)
        {
            try
            {
                var data = await _courseService.GetAllCourses(request);
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
        [HttpGet("GetCourse/{CourseId}")]
        public async Task<IActionResult> GetCourseById(int CourseId)
        {
            try
            {
                var data = await _courseService.GetCourseById(CourseId);
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
        [HttpPut("Status/{CourseId}")]
        public async Task<IActionResult> StatusActiveInactive(int CourseId)
        {
            try
            {
                var data = await _courseService.StatusActiveInactive(CourseId);
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
        [HttpGet("GetAllCoursesMasters")]
        public async Task<IActionResult> GetAllCoursesListMasters()
        {
            try
            {
                var data = await _courseService.GetAllCoursesMasters();
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
