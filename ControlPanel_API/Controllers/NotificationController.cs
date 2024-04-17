using ControlPanel_API.DTOs;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    //Notification template
    [Route("iGuru/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly INotificationServices _notificationServices;

        public NotificationController(INotificationServices notificationServices)
        {
            _notificationServices = notificationServices;
        }

        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetAllNotificationsList()
        {
            try
            {
                var storyOfTheDays = await _notificationServices.GetAllNotificationsList();
                return Ok(storyOfTheDays);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                var storyOfTheDay = await _notificationServices.GetNotificationById(id);
                return Ok(storyOfTheDay);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateFile")]
        public async Task<IActionResult> UpdateNotificationFile([FromForm] NotificationImageDTO request)
        {
            if (request.PathURL == null)
            {
                return BadRequest("The File field is required");
            }

            try
            {
                var data = await _notificationServices.UpdateNotificationFile(request);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetFile/{id}")]
        public async Task<IActionResult> GetNotificationFileById(int id)
        {
            try
            {
                var file = await _notificationServices.GetNotificationFileById(id);
                return File(file.Data, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
