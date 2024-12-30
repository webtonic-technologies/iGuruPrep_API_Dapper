using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/Registration")]
    [ApiController]
    [Authorize]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ICourseService _courseService;
        private readonly IClassCourseService _classCourseService; // Define the class course service
        private readonly IConfiguration _config;
        public RegistrationController(
            IRegistrationService registrationService,
            ICourseService courseService,
            IClassCourseService classCourseService, IConfiguration configuration) // Inject the class course service
        {
            _registrationService = registrationService;
            _courseService = courseService;
            _config = configuration;
            _classCourseService = classCourseService; // Assign the class course service
        }
        [AllowAnonymous]
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
        [HttpPost("AddUpdateProfile")]
        public async Task<IActionResult> AddUpdateProfile(UpdateProfileRequest request)
        {
            var response = await _registrationService.AddUpdateProfile(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCourse")]
        public async Task<IActionResult> GetCourses()
        {

            try
            {
                var IsLoginSuccessful = User.Claims.FirstOrDefault(c => c.Type == "IsLoginSuccessful")?.Value;
                if (IsLoginSuccessful == null || !bool.Parse(IsLoginSuccessful))
                {
                    return Forbid(); // or return Unauthorized();
                }

                return new OkObjectResult(await _courseService.GetCoursesAsync());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }
        [HttpGet("GetClass")]
        public async Task<IActionResult> GetClasses()
        {

            try
            {
                var IsLoginSuccessful = User.Claims.FirstOrDefault(c => c.Type == "IsLoginSuccessful")?.Value;
                if (IsLoginSuccessful == null || !bool.Parse(IsLoginSuccessful))
                {
                    return Forbid(); // or return Unauthorized();
                }

                return new OkObjectResult(await _courseService.GetClassesAsync());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }
        [HttpGet("GetBoard")]
        public async Task<IActionResult> GetBoards()
        {

            try
            {
                var IsLoginSuccessful = User.Claims.FirstOrDefault(c => c.Type == "IsLoginSuccessful")?.Value;
                if (IsLoginSuccessful == null || !bool.Parse(IsLoginSuccessful))
                {
                    return Forbid(); // or return Unauthorized();
                }

                return new OkObjectResult(await _courseService.GetBoardsAsync());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
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
        [HttpGet("GetProfileById/{registrationId}")]
        public async Task<IActionResult> GetRegistrationByIdAsync(int registrationId)
        {
            var response = await _registrationService.GetRegistrationByIdAsync(registrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("SendOTP")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var jwtToken = new JwtHelper(_config);
                var result = await _registrationService.LoginAsync(request);
                if (result.Data != null)
                {
                    var status = true;
                    var message = "Login successful";
                    string usertype = result.Data.IsEmployee == true ? "Employee" : "Student";
                    var token = jwtToken.GenerateJwtToken(result.Data.UserID, usertype, string.Empty, result.Data.IsLoginSuccessful);
                    var data = result;
                    return this.Ok(new { status, message, data, token });
                }
                else
                {
                    var status = false;
                    var message = "Invalid Username or Password";
                    return this.BadRequest(new { status, message });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }

        [HttpPost("AssignStudentClassCourseBoardMapping")]
        public async Task<IActionResult> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request)
        {
            var response = await _registrationService.AssignStudentClassCourseBoardMapping(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpDelete("DeleteProfile/{registrationId}")]
        public async Task<IActionResult> DeleteProfile(int registrationId)
        {
            var response = await _registrationService.DeleteProfile(registrationId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("DeviceCapture")]
        [AllowAnonymous]
        public async Task<IActionResult> DeviceCapture(DeviceCaptureRequest request)
        {
            var response = await _registrationService.DeviceCapture(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("GetAllClassCoursesMappings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllClassCoursesMappings(GetAllClassCourseRequest request)
        {
            var response = await _registrationService.GetAllClassCoursesMappings(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetCountries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountries()
        {
            var response = await _registrationService.GetCountries();
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStatesByCountryId/{countryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatesByCountryId(int countryId)
        {
            var response = await _registrationService.GetStatesByCountryId(countryId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPut("UserLogout")]
        public async Task<IActionResult> UserLogout(UserLogoutRequest request)
        {
            try
            {
                var data = await _registrationService.UserLogout(request);
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
        [HttpPost]
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string UserEmailOrPhoneOrUsername)
        {
            try
            {
                var result = await _registrationService.ForgetPasswordAsync(UserEmailOrPhoneOrUsername);
                var jwtToken = new JwtHelper(_config);
                if (result != null)
                {

                    var token = jwtToken.GenerateJwtToken(result.Data.UserId, result.Data.UserType, result.Data.UserName, true);
                    var email = new SendEmail();
                    result.Data.Token = token;
                    var succes = email.SendEmailWithAttachmentAsync(result.Data.Email, token);
                    return succes.Result.Success
                     ? this.Ok(new { result })
                     : this.BadRequest(new { result });

                }
                else
                {
                    var status = true;
                    var message = "Email not verified ";
                    return this.NotFound(new { status, message });
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
