using Config_API.Services.Interfaces;
using iGuruPrep.Models;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectServices _subjectService;

        public SubjectController(ISubjectServices subjectServices)
        {
            _subjectService = subjectServices;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateSubject(Subject request)
        {
            try
            {
                var data = await _subjectService.AddUpdateSubject(request);
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
        [HttpGet("GetAllSubjects")]
        public async Task<IActionResult> GetAllSubjectsList()
        {
            try
            {
                var data = await _subjectService.GetAllSubjects();
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
        [HttpGet("GetSubject/{SubjectId}")]
        public async Task<IActionResult> GetSubjectById(int SubjectId)
        {
            try
            {
                var data = await _subjectService.GetSubjectById(SubjectId);
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
        [HttpPut("Status/{SubjectId}")]
        public async Task<IActionResult> StatusActiveInactive(int SubjectId)
        {
            try
            {
                var data = await _subjectService.StatusActiveInactive(SubjectId);
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
