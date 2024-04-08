using ControlPanel_API.DTOs;
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
        [HttpPost]
        public async Task<IActionResult> AddNewMagazine([FromForm] MagazineDTO magazineDTO)
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

        [HttpPut]
        public async Task<IActionResult> UpdateMagazine([FromForm] UpdateMagazineDTO magazineDTO)
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

        [HttpGet]
        public async Task<IActionResult> GetAllMagazines()
        {
            try
            {
                var magazines = await _magazineService.GetAllMagazines();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMagazine(int id)
        {
            try
            {
                var data = await _magazineService.DeleteMagazine(id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateFile")]
        public async Task<IActionResult> UpdateMagazineFile([FromForm] MagazineDTO magazineDTO)
        {
            if (magazineDTO.File == null)
            {
                return BadRequest("The File field is required");
            }

            try
            {
                var data = await _magazineService.UpdateMagazineFile(magazineDTO);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetFile/{id}")]
        public async Task<IActionResult> GetMagazineFileById(int id)
        {
            try
            {
                var file = await _magazineService.GetMagazineFileById(id);
                return File(file.Data, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
