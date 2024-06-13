using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/ControlPanel/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {

        private readonly IFeedbackServices _feedbackService;

        public FeedbackController(IFeedbackServices feedbackServices)
        {
            _feedbackService = feedbackServices;
        }

        [HttpPost("GetAllFeedback")]
        public async Task<IActionResult> GetAllFeedback(GetAllFeedbackRequest request)
        {
            try
            {
                var data = await _feedbackService.GetAllFeedBackList(request);
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
