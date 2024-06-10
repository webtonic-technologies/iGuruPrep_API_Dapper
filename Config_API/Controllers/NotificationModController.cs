using Config_API.DTOs.Requests;
using Config_API.Models;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class NotificationModController : ControllerBase
    {
        private readonly INotificationModServices _notificationModServices;

        public NotificationModController(INotificationModServices notificationModServices)
        {
            _notificationModServices = notificationModServices;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateNotification(NotificationDTO request)
        {
            try
            {
                var data = await _notificationModServices.AddUpdateNotification(request);
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
        [HttpGet("GetAllPlatforms")]
        public async Task<IActionResult> GetAllPlatforms()
        {
            try
            {
                var data = await _notificationModServices.GetAllPlatformList();
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
        [HttpGet("GetNotification/{Id}")]
        public async Task<IActionResult> GetNotificationsById(int Id)
        {
            try
            {
                var data = await _notificationModServices.GetNotificationsById(Id);
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
        [HttpPut("Status/{Id}")]
        public async Task<IActionResult> StatusActiveInactive(int Id)
        {
            try
            {
                var data = await _notificationModServices.StatusActiveInactive(Id);
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
        [HttpGet("GetAllModules")]
        public async Task<IActionResult> GetAllModules()
        {
            try
            {
                var data = await _notificationModServices.GetAllModuleList();
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
        [HttpPost("GetNotifications")]
        public async Task<IActionResult> GetListOfNotifications(GetAllNotificationModRequest request)
        {
            try
            {
                var data = await _notificationModServices.GetListofNotifications(request);
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
