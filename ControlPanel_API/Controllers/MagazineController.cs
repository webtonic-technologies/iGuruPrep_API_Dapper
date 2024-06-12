using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Implementations;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class MagazineController : ControllerBase
    {

        private readonly IMagazineServices  _magazineService;

        public MagazineController(IMagazineServices magazineServices)
        {
            _magazineService = magazineServices;
        }
        [HttpPost("AddMagazine")]
        public async Task<IActionResult> AddNewMagazine([FromBody] MagazineDTO magazineDTO)
        {
            try
            {
                var data = await _magazineService.AddNewMagazine(magazineDTO);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateMagazine")]
        public async Task<IActionResult> UpdateMagazine([FromBody] MagazineDTO magazineDTO)
        {
            try
            {
                var data = await _magazineService.UpdateMagazine(magazineDTO);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMagazineById(int id)
        {
            try
            {
                var magazine = await _magazineService.GetMagazineById(id);
                return Ok(magazine);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Status/{id}")]
        public async Task<IActionResult> StatusActiveInactive(int id)
        {
            try
            {
                var data = await _magazineService.StatusActiveInactive(id);
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
