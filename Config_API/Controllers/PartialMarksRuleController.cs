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
        [HttpGet("GetAllPartialMarksRule")]
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
        [HttpPost("GetAllPartialMarksRulesList")]
        public async Task<IActionResult> GetAllPartialMarksRulesList(GetListRequest request)
        {
            try
            {
                var data = await _partialMarksRuleServices.GetAllPartialMarksRulesList(request);
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
        [HttpGet("download-partial-marks/{ruleId}")]
        public async Task<IActionResult> DownloadPartialMarksExcel(int ruleId)
        {
            try
            {
                var fileContent = await _partialMarksRuleServices.DownloadPartialMarksExcelSheet(ruleId);

                // Return the Excel file as a downloadable response
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PartialMarksData.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}