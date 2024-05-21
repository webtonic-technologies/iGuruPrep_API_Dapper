using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Repository.Interfaces
{
    public interface IProjectRepository
    {
        Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO projectDTO);
        Task<ServiceResponse<List<ProjectDTO>>> GetAllProjectsByFilter(ProjectFilter filter);
        Task<ServiceResponse<ProjectDTO>> GetProjectByIdAsync(int projectId);
    }
}
