using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class InstructionsController : ControllerBase
    {
        private readonly IInstructionsServices _instructionsServices;

        public InstructionsController(IInstructionsServices instructionsServices)
        {
            _instructionsServices = instructionsServices;
        }
        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateInstruction(Instructions request)
        {
            try
            {
                var data = await _instructionsServices.AddUpdateInstruction(request);
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
        [HttpPost("GetAllInstructions")]
        public async Task<IActionResult> GetAllInstructions(GetAllInstructionsRequest request)
        {
            try
            {
                var data = await _instructionsServices.GetAllInstructions(request);
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
        [HttpGet("GetInstructionById/{InstructionId}")]
        public async Task<IActionResult> GetInstructionById(int InstructionId)
        {
            try
            {
                var data = await _instructionsServices.GetInstructionById(InstructionId);
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
        [HttpGet("GetAllInstructionsMasters")]
        public async Task<IActionResult> GetAllInstructionsMaster()
        {
            try
            {
                var data = await _instructionsServices.GetAllInstructionsMaster();
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
