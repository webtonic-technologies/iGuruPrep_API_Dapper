using ControlPanel_API.DTOs;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class HelpFAQController : ControllerBase
    {

        private readonly IHelpFAQServices _helpFAQServices;

        public HelpFAQController(IHelpFAQServices helpFAQServices)
        {
            _helpFAQServices = helpFAQServices;
        }

        [HttpPost("GetFAQs")]
        public async Task<IActionResult> GetListOfFAQ(GetAllFAQRequest request)
        {
            try
            {
                return new OkObjectResult(await _helpFAQServices.GetFAQList(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }
        [HttpGet("GetFAQById/{FAQId}")]
        public async Task<IActionResult> GetDesignationByID(int FAQId)
        {
            try
            {
                return new OkObjectResult(await _helpFAQServices.GetFAQById(FAQId));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

        }

        [HttpPost("AddUpdateHelpFAQ")]
        public async Task<IActionResult> AddUpdateHelpFAQ(HelpFAQ request)
        {
            try
            {
                return new OkObjectResult(await _helpFAQServices.AddUpdateFAQ(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

        }
        [HttpPut("Status/{FAQId}")]
        public async Task<IActionResult> StatusActiveInactive(int FAQId)
        {
            try
            {
                var data = await _helpFAQServices.StatusActiveInactive(FAQId);
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
