using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class DesignationController : ControllerBase
    {

        private readonly IDesignationServices _designationService;

        public DesignationController(IDesignationServices designationServices)
        {
            _designationService = designationServices;
        }
        [HttpGet("GetDesignationMasters")]
        public async Task<IActionResult> GetDesignationMasters()
        {
            try
            {
                return new OkObjectResult(await _designationService.GetDesignationListMasters());
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }

        [HttpPost("GetDesignation")]
        public async Task<IActionResult> GetDesignation(GetAllDesignationsRequest request)
        {
            try
            {
                return new OkObjectResult(await _designationService.GetDesignationList(request) );
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
                return new OkObjectResult(await _designationService.GetDesignationByID(DesgnID));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }

        [HttpPost("AddUpdateDesignation")]
        public async Task<IActionResult> AddUpdateDesignation(Designation designation)
        {
            try
            {
                return new OkObjectResult(await _designationService.AddUpdateDesignation(designation));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPut("Status/{DesignationId}")]
        public async Task<IActionResult> StatusActiveInactive(int DesignationId)
        {
            try
            {
                var data = await _designationService.StatusActiveInactive(DesignationId);
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
