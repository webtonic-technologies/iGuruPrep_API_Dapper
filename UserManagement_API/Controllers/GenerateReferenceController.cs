using Microsoft.AspNetCore.Mvc;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Controllers
{
    [Route("iGuru/UserManagement/[controller]")]
    [ApiController]
    public class GenerateReferenceController : ControllerBase
    {
        private readonly IGenerateReferenceServices _generateReferenceServices;

        public GenerateReferenceController(IGenerateReferenceServices generateReferenceServices)
        {
            _generateReferenceServices = generateReferenceServices;
        }

        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateGenerateReference(GenerateReferenceDTO request)
        {
            try
            {
                var data = await _generateReferenceServices.AddUpdateGenerateReference(request);
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
        [HttpGet("GetReferrelById/{referenceLinkID}")]
        public async Task<IActionResult> GetGenerateReferenceById(int referenceLinkID)
        {
            try
            {
                var data = await _generateReferenceServices.GetGenerateReferenceById(referenceLinkID);
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
        [HttpPost("GetAllReferrals")]
        public async Task<IActionResult> GetGenerateReferenceList(GetAllReferralsRequest request)
        {
            try
            {
                var data = await _generateReferenceServices.GetGenerateReferenceList(request);
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
        [HttpGet("GetBankList")]
        public async Task<IActionResult> GetBankList()
        {
            try
            {
                var data = await _generateReferenceServices.GetBankListMasters();
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
        [HttpGet("GetStatesList")]
        public async Task<IActionResult> GetStatesList()
        {
            try
            {
                var data = await _generateReferenceServices.GetStatesListMasters();
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
        [HttpGet("GetDistrictsList/{StateId}")]
        public async Task<IActionResult> GetDistrictsList(int StateId)
        {
            try
            {
                var data = await _generateReferenceServices.GetDistrictsListMasters(StateId);
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
