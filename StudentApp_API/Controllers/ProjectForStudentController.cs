using Microsoft.AspNetCore.Mvc;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Controllers
{
    [Route("iGuru/StudentApp/ProjectForStudent")]
    [ApiController]
    public class ProjectForStudentController : ControllerBase
    {
        private readonly IProjectForStudentsServices _projectForStudentsService;

        public ProjectForStudentController(IProjectForStudentsServices projectForStudentsServices) // Inject the class course service
        {
            _projectForStudentsService = projectForStudentsServices;
           
        }
        [HttpPost("GetAllProjects")]
        public async Task<IActionResult> GetAllProjects(ProjectForStudentsRequest request)
        {
            var response = await _projectForStudentsService.GetAllProjects(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("GetProjectSubjects")]
        public async Task<IActionResult> GetSubjectProjectCounts(ProjectForStudentRequest request)
        {
            var response = await _projectForStudentsService.GetSubjectProjectCounts(request);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [HttpGet("GetProjectInfo/{projectId}")]
        public async Task<IActionResult> GetProjectByIdAsync(int projectId)
        {
            var response = await _projectForStudentsService.GetProjectByIdAsync(projectId);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
