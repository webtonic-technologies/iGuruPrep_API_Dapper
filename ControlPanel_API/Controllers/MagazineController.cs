using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class MagazineController : ControllerBase
    {

        private readonly IMagazineServices  _magazineService;

        public MagazineController(IMagazineServices magazineServices)
        {
            _magazineService = magazineServices;
        }
        [HttpPost("AddUpdateMagazine")]
        public async Task<IActionResult> AddUpdateMagazine([FromBody] MagazineDTO magazineDTO)
        {
            try
            {
                var data = await _magazineService.AddUpdateMagazine(magazineDTO);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetListOfMagazines")]
        public async Task<IActionResult> GetAllMagazines(MagazineListDTO request)
        {
            try
            {
                var magazines = await _magazineService.GetAllMagazines(request);
                return Ok(magazines);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetMagazineById/{MagazineId}")]
        public async Task<IActionResult> GetMagazineById(int MagazineId)
        {
            try
            {
                var magazine = await _magazineService.GetMagazineById(MagazineId);
                return Ok(magazine);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Status/{MagazineId}")]
        public async Task<IActionResult> StatusActiveInactive(int MagazineId)
        {
            try
            {
                var data = await _magazineService.StatusActiveInactive(MagazineId);
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
        //[HttpGet("PublishMagazine")]
        //public async Task<IActionResult> GetMagazineByPublishDate()
        //{
        //    try
        //    {
        //        var magazine = await _magazineService.GetMagazineByPublishDate();
        //        return Ok(magazine);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
