using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {

        private readonly IEmployeeServices _employeeServices;
        private readonly IConfiguration _config;
        public EmployeeController(IEmployeeServices employeeServices, IConfiguration configuration)
        {
            _employeeServices = employeeServices;
            _config = configuration;
        }

        [HttpPost("GetEmployee")]
        public async Task<IActionResult> GetEmployeeList(GetEmployeeListDTO request)
        {
            //try
            //{
            //    return new OkObjectResult(await _employeeServices.GetEmployeeList(request));
            //}
            //catch (Exception ex)
            //{
            //    return new JsonResult(ex.Message)
            //    {
            //        StatusCode = (int)HttpStatusCode.NotFound
            //    };
            //}
            try
            {
                // Check if the user is a superadmin
                var isSuperAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsSuperAdmin")?.Value;
                if (isSuperAdmin == null || !bool.Parse(isSuperAdmin))
                {
                    return Forbid(); // or return Unauthorized();
                }

                return new OkObjectResult(await _employeeServices.GetEmployeeList(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }
        [HttpGet("GetEmployeeById/{Employeeid}")]
        public async Task<IActionResult> GetEmployeeByID(int Employeeid)
        {
            try
            {
                return new OkObjectResult(await _employeeServices.GetEmployeeByID(Employeeid));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }
        [HttpPost("AddUpdateEmployee")]
        public async Task<IActionResult> AddUpdateEmployee(EmployeeDTO request)
        {
            try
            {
                return new OkObjectResult(await _employeeServices.AddUpdateEmployee(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPut("Status/{Id}")]
        public async Task<IActionResult> StatusActiveInactive(int Id)
        {
            try
            {
                var data = await _employeeServices.StatusActiveInactive(Id);
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
        [HttpPost("EmployeeLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> EmployeeLogin(EmployeeLoginRequest request)
        {
            try
            {
                var jwtToken = new JwtHelper(_config);
                var result = await _employeeServices.EmployeeLogin(request);
                if (result != null)
                {
                    var status = true;
                    var message = "Login successful";
                    var token = jwtToken.GenerateJwtToken(result.Data.Employeeid,result.Data.RoleName, true);
                    var data = result;
                    return this.Ok(new { status, message, data, token });
                }
                else
                {
                    var status = false;
                    var message = "Login failed";
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
        [HttpPost("DeviceCapture")]
        public async Task<IActionResult> DeviceCapture(DeviceCaptureRequest request)
        {
            try
            {
                return new OkObjectResult(await _employeeServices.DeviceCapture(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPut("UserLogout/{userId}")]
        public async Task<IActionResult> UserLogout(int userId)
        {
            try
            {
                var data = await _employeeServices.UserLogout(userId);
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
        [AllowAnonymous]
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin(UserLoginRequest request)
        {
            try
            {
                return new OkObjectResult(await _employeeServices.UserLogin(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
    }
}
