using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {

        private readonly IRolesServices _rolesService;

        public RolesController(IRolesServices rolesServices)
        {
            _rolesService = rolesServices;
        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                return new OkObjectResult(new { data = await _rolesService.GetRoles() });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }
        [HttpGet("GetRoleById/{roleId}")]
        public async Task<IActionResult> GetRoleByID(int roleId)
        {
            try
            {
                return new OkObjectResult(new { data = await _rolesService.GetRoleByID(roleId) });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }

        [HttpPost("AddRole")]
        public async Task<ActionResult> AddRole(Role role)
        {
            try
            {
                return new OkObjectResult(new { data = await _rolesService.AddRole(role) });
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPost("UpdateRole")]
        public async Task<ActionResult> UpdateRole(Role role)
        {
            try
            {
                if (role.RoleId != 0)
                {
                    return new OkObjectResult(new { data = await _rolesService.UpdateRole(role) });

                }
                return NotFound();
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