using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class TimeTablePreparationController : ControllerBase
    {

        private readonly ITimeTablePreparationServices _timeTablePreparationServices;

        public TimeTablePreparationController(ITimeTablePreparationServices timeTablePreparationServices)
        {
            _timeTablePreparationServices = timeTablePreparationServices;
        }

        [HttpPost("GetAllTimeTableList")]
        public async Task<IActionResult> GetAllTimeTableList(TimeTableListRequestDTO request)
        {
            try
            {
                var data = await _timeTablePreparationServices.GetAllTimeTableList(request);
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
        [HttpGet("GetTimeTableById/{PreparationTimeTableId}")]
        public async Task<IActionResult> GetAllTimeTableList(int PreparationTimeTableId)
        {
            try
            {
                var data = await _timeTablePreparationServices.GetTimeTableById(PreparationTimeTableId);
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
        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateTimeTablePreparation(TimeTablePreparationRequest request)
        {
            try
            {
                var data = await _timeTablePreparationServices.AddUpdateTimeTable(request);
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
