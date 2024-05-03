using ControlPanel_API.DTOs;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly IEmployeeServices _employeeServices;

        public EmployeeController(IEmployeeServices employeeServices)
        {
            _employeeServices = employeeServices;
        }

        [HttpPost("GetEmployee")]
        public async Task<IActionResult> GetEmployeeList(GetEmployeeListDTO request)
        {
            try
            {
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
        [HttpGet("GetEmployeeById/{ID}")]
        public async Task<IActionResult> GetEmployeeByID(int ID)
        {
            try
            {
                return new OkObjectResult(await _employeeServices.GetEmployeeByID(ID));
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
        public async Task<IActionResult> AddUpdateEmployee(Employee request)
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
    }
}
