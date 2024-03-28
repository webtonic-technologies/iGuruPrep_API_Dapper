using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Repository.Interfaces;
using Schools_API.Services.Interfaces;

namespace Schools_API.Services.Implementations
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectServices(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }
        public async Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO projectDTO)
        {
            try
            {
                return await _projectRepository.AddProjectAsync(projectDTO);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<IEnumerable<ProjectDTO>>> GetAllProjectsByFilter(ProjectFilter filter)
        {

            try
            {
                return await _projectRepository.GetAllProjectsByFilter(filter);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<ProjectDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<ProjectDetailsDTO>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                return await _projectRepository.GetProjectByIdAsync(projectId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ProjectDetailsDTO>(false, ex.Message, new ProjectDetailsDTO(), 500);
            }
        }
    }
}
