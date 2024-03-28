using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class DesignationController : ControllerBase
    {

        private readonly IDesignationServices _designationService;

        public DesignationController(IDesignationServices designationServices)
        {
            _designationService = designationServices;
        }

        [HttpGet("GetDesignation")]
        public async Task<IActionResult> GetDesignation()
        {
            try
            {
                return new OkObjectResult(new { data = await _designationService.GetDesignationList() });
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }
        [HttpGet("GetDesignationById/{DesgnID}")]
        public async Task<IActionResult> GetDesignationByID(int DesgnID)
        {
            try
            {
                return new OkObjectResult(new { data = await _designationService.GetDesignationByID(DesgnID) });
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }

        [HttpPost("AddDesignation")]
        public async Task<IActionResult> AddDesignation(Designation designation)
        {
            try
            {
                return new OkObjectResult(new { data = await _designationService.AddDesignation(designation) });
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPost("UpdateDesignation")]
        public async Task<ActionResult> UpdateDesignation(Designation designation)
        {
            try
            {
                if (designation.DesgnID != 0)
                {
                    return new OkObjectResult(new { data = await _designationService.UpdateDesignation(designation) });

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
