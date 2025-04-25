using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Implementations;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/StudentSubscription")]
    [ApiController]
    public class StudentSubscriptionController : ControllerBase
    {
        private readonly IStudentSubscriptionServices _studentSubscriptionServices;

        public StudentSubscriptionController(IStudentSubscriptionServices studentSubscriptionServices) // Inject the class course service
        {
            _studentSubscriptionServices = studentSubscriptionServices;

        }
        [HttpPost("GetSubscriptionPackages")]
        public async Task<IActionResult> GetSubscriptionPackages(SubscriptionRequestDTO request)
        {
            var response = await _studentSubscriptionServices.GetSubscriptionPackages(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("InsertStudentSubscription")]
        public async Task<IActionResult> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request)
        {
            var response = await _studentSubscriptionServices.InsertStudentSubscriptionAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpPost("InitiateTransaction")]
        public async Task<IActionResult> InitiateTransactionAsync(InitiateTransactionRequest request)
        {
            var response = await _studentSubscriptionServices.InitiateTransactionAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetTransactionDetailsByOrderId")]
        public async Task<IActionResult> GetTransactionDetailsByOrderIdAsync(string orderId)
        {
            var response = await _studentSubscriptionServices.GetTransactionDetailsByOrderIdAsync(orderId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetStudentCoinTransactions/{studentId}")]
        public async Task<IActionResult> GetStudentCoinTransactionsAsync(int studentId)
        {
            var response = await _studentSubscriptionServices.GetStudentCoinTransactionsAsync(studentId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpPost("CoinTransaction")]
        public async Task<IActionResult> CoinTransactionAsync(CoinTransactionRequestDTO request)
        {
            var response = await _studentSubscriptionServices.CoinTransactionAsync(request);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}