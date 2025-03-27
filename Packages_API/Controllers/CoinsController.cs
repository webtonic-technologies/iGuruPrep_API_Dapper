using Microsoft.AspNetCore.Mvc;
using Packages_API.Models;
using Packages_API.Services.Interfaces;

namespace Packages_API.Controllers
{
    [ApiController]
    [Route("api/Coins")]
    public class CoinsController : ControllerBase
    {
        private readonly ICoinsServices _coinsServices;

        public CoinsController(ICoinsServices coinsServices)
        {
            _coinsServices = coinsServices;
        }

        [HttpPost("AddUpdateCoins")]
        public async Task<IActionResult> AddUpdateCoins([FromBody] Coins coin)
        {
            var response = await _coinsServices.AddUpdateCoins(coin);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("GetAllCoins")]
        public async Task<IActionResult> GetAllCoins()
        {
            var response = await _coinsServices.GetAllCoins();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetCoin/{coinId}")]
        public async Task<IActionResult> GetCoin(int coinId)
        {
            var response = await _coinsServices.GetCoin(coinId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("DeleteCoin/{coinId}")]
        public async Task<IActionResult> DeleteCoin(int coinId)
        {
            var response = await _coinsServices.DeleteCoin(coinId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("CoinStatus/{coinId}")]
        public async Task<IActionResult> CoinStatus(int coinId)
        {
            var response = await _coinsServices.CoinStatus(coinId);
            return StatusCode(response.StatusCode, response);
        }
    }

}
