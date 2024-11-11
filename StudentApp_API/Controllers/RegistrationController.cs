using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;
using System.Threading.Tasks;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/Registration")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ICourseService _courseService;
        private readonly IClassCourseService _classCourseService; // Define the class course service

        public RegistrationController(
            IRegistrationService registrationService,
            ICourseService courseService,
            IClassCourseService classCourseService) // Inject the class course service
        {
            _registrationService = registrationService;
            _courseService = courseService;
            _classCourseService = classCourseService; // Assign the class course service
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegistrationRequest request)
        {
            var response = await _registrationService.RegisterStudentAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("GetCourse")]
        public async Task<IActionResult> GetCourses()
        {
            var response = await _courseService.GetCoursesAsync();
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetClassCourseID/{courseId}")]
        public async Task<IActionResult> GetClassesByCourseId(int courseId)
        {
            var response = await _classCourseService.GetClassesByCourseIdAsync(courseId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }


        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            var response = await _registrationService.SendOTPAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            var response = await _registrationService.VerifyOTPAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _registrationService.LoginAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("AssignCourse")]
        public async Task<IActionResult> AssignCourse([FromBody] AssignCourseRequest request)
        {
            var response = await _registrationService.AssignCourseAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("AssignClass")]
        public async Task<IActionResult> AssignClass([FromBody] AssignClassRequest request)
        {
            var response = await _registrationService.AssignClassAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
