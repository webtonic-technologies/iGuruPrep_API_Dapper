using Microsoft.AspNetCore.Mvc;
using Schools_API.DTOs.Requests;
using Schools_API.Services.Interfaces;

namespace Schools_API.Controllers
{
    [Route("iGuru/Schools/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }
        [HttpPost("AddUpdateProject")]
        public async Task<IActionResult> AddProject([FromBody] ProjectDTO projectDTO)
        {
            try
            {

                if (projectDTO == null)
                {
                    return BadRequest("Project data is null.");
                }

                var createdProject = await _projectServices.AddProjectAsync(projectDTO);

                if (createdProject == null)
                {
                    return StatusCode(500, "A problem happened while handling your request.");
                }

                return Ok(createdProject);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpPost("GetAllProjects")]
        public async Task<IActionResult> GetAllProjects(ProjectFilter filter)
        {
            var projects = await _projectServices.GetAllProjectsByFilter(filter);

            if (projects == null)
            {
                return NotFound("No projects found.");
            }

            return Ok(projects);
        }
        [HttpGet("GetProjectById/{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId)
        {
            var project = await _projectServices.GetProjectByIdAsync(projectId);

            if (project == null)
            {
                return NotFound("Project not found.");
            }

            return Ok(project);
        }
    }
}
