using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class PartialMarksRuleController : ControllerBase
    {
        private readonly IPartialMarksRuleServices _partialMarksRuleServices;

        public PartialMarksRuleController(IPartialMarksRuleServices partialMarksRuleServices)
        {
            _partialMarksRuleServices = partialMarksRuleServices;
        }
        [HttpPost("AddPartialMarkRule")]
        public async Task<IActionResult> AddPartialMarksRule(PartialMarksRequest request)
        {
            try
            {
                var response = await _partialMarksRuleServices.AddPartialMarksRule(request);
                if (response.Success)
                {
                    return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PartialMarksRule.xlsx");
                }
                return StatusCode(response.StatusCode, response.Message);

            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }

        }
        [HttpPost("GetAllPartialMarksRule")]
        public async Task<IActionResult> GetAllPartialMarksRules()
        {
            try
            {
                var data = await _partialMarksRuleServices.GetAllPartialMarksRules();
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
        [HttpGet("GetPartialMarksRuleyId/{RuleId}")]
        public async Task<IActionResult> GetPartialMarksRuleyId(int RuleId)
        {
            try
            {
                var data = await _partialMarksRuleServices.GetPartialMarksRuleyId(RuleId);
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
        [HttpPut("UploadPartialMarksSheet/{RuleId}")]
        public async Task<IActionResult> UploadPartialMarksSheet(IFormFile file, int RuleId)
        {
            try
            {
                var data = await _partialMarksRuleServices.UploadPartialMarksSheet(file, RuleId);
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