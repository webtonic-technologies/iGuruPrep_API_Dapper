using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Services.Interfaces
{
    public interface IProjectServices
    {
        Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO projectDTO);
        Task<ServiceResponse<List<ProjectResponseDTO>>> GetAllProjectsByFilter(ProjectFilter filter);
        Task<ServiceResponse<ProjectResponseDTO>> GetProjectByIdAsync(int projectId);
    }
}
