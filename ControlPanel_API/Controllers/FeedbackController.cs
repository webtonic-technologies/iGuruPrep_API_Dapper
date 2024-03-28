using ControlPanel_API.DTOs;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {

        private readonly IFeedbackServices _feedbackService;

        public FeedbackController(IFeedbackServices feedbackServices)
        {
            _feedbackService = feedbackServices;
        }
        [HttpPost("AddFeedback")]
        public async Task<IActionResult> AddFeedback(Feedback request)
        {
            try
            {
                var data = await _feedbackService.AddFeedBack(request);
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
        [HttpPost("AddSyllabus")]
        public async Task<IActionResult> AddSyllabus(Syllabus request)
        {
            try
            {
                var data = await _feedbackService.AddSyllabus(request);
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
        [HttpPut("UpdateFeedback")]
        public async Task<IActionResult> UpdateFeedback(Feedback request)
        {
            try
            {
                var data = await _feedbackService.UpdateFeedback(request);
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
