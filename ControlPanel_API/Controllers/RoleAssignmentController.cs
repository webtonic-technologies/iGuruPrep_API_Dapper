using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class RoleAssignmentController : ControllerBase
    {

        private readonly IRoleAssignmentServices _roleAssignmentServices;

        public RoleAssignmentController(IRoleAssignmentServices roleAssignmentServices)
        {
            _roleAssignmentServices = roleAssignmentServices;
        }


        [HttpPost("GetMasterMenu")]
        public async Task<IActionResult> GetMasterMenu()
        {
            try
            {
                return new OkObjectResult(await _roleAssignmentServices.GetMasterMenu());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }
        }

        [HttpPost("AddUpdateRoleAssignment")]
        public async Task<IActionResult> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId)
        {
            try
            {
                return new OkObjectResult(await _roleAssignmentServices.AddUpdateRoleAssignment(request, EmployeeId));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }
        }
        [HttpPut("RemoveRoleAssignment/{RAMappingId}")]
        public async Task<IActionResult> RemoveRoleAssignment(int RAMappingId)
        {
            try
            {
                var data = await _roleAssignmentServices.RemoveRoleAssignment(RAMappingId);
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
