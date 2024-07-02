using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Implementations;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    //Notification template
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly INotificationServices _notificationServices;

        public NotificationController(INotificationServices notificationServices)
        {
            _notificationServices = notificationServices;
        }

        [HttpPost("AddUpdate")]
        public async Task<IActionResult> AddUpdateNotification([FromBody]NotificationDTO request)
        {
            try
            {
                var data = await _notificationServices.AddUpdateNotification(request);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetNotificationsList")]
        public async Task<IActionResult> GetAllNotificationsList(NotificationsListDTO request)
        {
            try
            {
                var storyOfTheDays = await _notificationServices.GetAllNotificationsList(request);
                return Ok(storyOfTheDays);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetNotificationById/{NBNotificationID}")]
        public async Task<IActionResult> GetNotificationById(int NBNotificationID)
        {
            try
            {
                var storyOfTheDay = await _notificationServices.GetNotificationById(NBNotificationID);
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Status/{NBNotificationID}")]
        public async Task<IActionResult> StatusActiveInactive(int NBNotificationID)
        {
            try
            {
                var data = await _notificationServices.StatusActiveInactive(NBNotificationID);
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
