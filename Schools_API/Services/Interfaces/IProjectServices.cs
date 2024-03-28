using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Services.Interfaces
{
    public interface IProjectServices
    {
        Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO projectDTO);
        Task<ServiceResponse<IEnumerable<ProjectDTO>>> GetAllProjectsByFilter(ProjectFilter filter);
        Task<ServiceResponse<ProjectDetailsDTO>> GetProjectByIdAsync(int projectId);
    }
}
