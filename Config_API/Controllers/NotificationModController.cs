using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class NotificationModController : ControllerBase
    {
        private readonly INotificationModServices _notificationModServices;

        public NotificationModController(INotificationModServices notificationModServices)
        {
            _notificationModServices = notificationModServices;
        }
        [HttpPost("AddUpdate")]
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
        [HttpGet("GetNotificationById/{notificationTemplateID}")]
        public async Task<IActionResult> GetNotificationsById(int notificationTemplateID)
        {
            try
            {
                var data = await _notificationModServices.GetNotificationsById(notificationTemplateID);
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
        [HttpPut("Status/{notificationTemplateID}")]
        public async Task<IActionResult> StatusActiveInactive(int notificationTemplateID)
        {
            try
            {
                var data = await _notificationModServices.StatusActiveInactive(notificationTemplateID);
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
