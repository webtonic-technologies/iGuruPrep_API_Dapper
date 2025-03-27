using Microsoft.AspNetCore.Mvc;
using Packages_API.DTOs.Requests;
using Packages_API.Models;
using Packages_API.Services.Interfaces;

namespace Packages_API.Controllers
{
    [ApiController]
    [Route("api/SubscriptionPackage")]
    public class SubscriptionPackageController : ControllerBase
    {
        private readonly ISubscriptionPackageServices _subscriptionPackageServices;

        public SubscriptionPackageController(ISubscriptionPackageServices subscriptionPackageServices)
        {
            _subscriptionPackageServices = subscriptionPackageServices;
        }

        /// <summary>
        /// Add or Update a Subscription
        /// </summary>
        [HttpPost("AddUpdateSubscription")]
        public async Task<IActionResult> AddUpdateSubscription([FromBody] AddUpdateSubscriptionRequest subscription)
        {
            var response = await _subscriptionPackageServices.AddUpdateSubscription(subscription);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get all subscriptions
        /// </summary>
        [HttpGet("GetAllSubscriptions")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var response = await _subscriptionPackageServices.GetAllSubscriptions();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get all countries
        /// </summary>
        [HttpGet("GetAllCountry")]
        public async Task<IActionResult> GetAllCountry()
        {
            var response = await _subscriptionPackageServices.GetAllCountry();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get subjects by Board, Class, and Course
        /// </summary>
        [HttpPost("GetSubjectsByBoardClassCourse")]
        public async Task<IActionResult> GetSubjectsByBoardClassCourse([FromBody] SubjectRequestDTO request)
        {
            var response = await _subscriptionPackageServices.GetSubjectsByBoardClassCourse(request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get subscription by ID
        /// </summary>
        [HttpGet("GetSubscriptionByID/{subscriptionID}")]
        public async Task<IActionResult> GetSubscriptionByID(int subscriptionID)
        {
            var response = await _subscriptionPackageServices.GetSubscriptionByID(subscriptionID);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete a subscription
        /// </summary>
        [HttpDelete("DeleteSubscription/{subscriptionID}")]
        public async Task<IActionResult> DeleteSubscription(int subscriptionID)
        {
            var response = await _subscriptionPackageServices.DeleteSubscription(subscriptionID);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Change subscription status
        /// </summary>
        [HttpPut("SubscriptionStatus/{subscriptionID}")]
        public async Task<IActionResult> SubscriptionStatus(int subscriptionID)
        {
            var response = await _subscriptionPackageServices.SubscriptionStatus(subscriptionID);
            return StatusCode(response.StatusCode, response);
        }
    }
}