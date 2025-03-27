using Microsoft.AspNetCore.Mvc;
using Packages_API.DTOs.Requests;
using Packages_API.Models;
using Packages_API.Services.Interfaces;

namespace Packages_API.Controllers
{
    [ApiController]
    [Route("api/CoinsConfiguration")]
    public class CoinsConfigurationController : ControllerBase
    {
        private readonly ICoinsConfigurationServices _coinsConfigurationServices;

        public CoinsConfigurationController(ICoinsConfigurationServices coinsConfigurationServices)
        {
            _coinsConfigurationServices = coinsConfigurationServices;
        }

        [HttpPost("AddUpdateCoinConfiguration")]
        public async Task<IActionResult> AddUpdateCoinConfiguration([FromBody] AddUpdateCoinConfigurationRequest request)
        {
            var response = await _coinsConfigurationServices.AddUpdateCoinConfiguration(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllCoinConfiguration")]
        public async Task<IActionResult> GetAllCoinConfiguration()
        {
            var response = await _coinsConfigurationServices.GetAllCoinConfigurations();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetCoinConfigurationByID/{ccid}")]
        public async Task<IActionResult> GetCoinConfigurationByID(int ccid)
        {
            var response = await _coinsConfigurationServices.GetCoinConfigurationByID(ccid);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("CoinConfigurationStatus/{ccid}")]
        public async Task<IActionResult> CoinConfigurationStatus(int ccid)
        {
            var response = await _coinsConfigurationServices.CoinConfigurationStatus(ccid);
            return StatusCode(response.StatusCode, response);
        }
    }
}
