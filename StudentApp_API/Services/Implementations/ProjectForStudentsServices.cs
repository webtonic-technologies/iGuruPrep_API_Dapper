using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class ProjectForStudentsServices : IProjectForStudentsServices
    {
        private readonly IProjectForStudentsRepository _projectForStudentsRepository;

        public ProjectForStudentsServices(IProjectForStudentsRepository projectForStudentsRepository)
        {
            _projectForStudentsRepository = projectForStudentsRepository;
        }
        public async Task<ServiceResponse<List<ProjectForStudentsResponse>>> GetAllProjects(ProjectForStudentsRequest request)
        {
            return await _projectForStudentsRepository.GetAllProjects(request);
        }

        public async Task<ServiceResponse<ProjectForStudentsResponse>> GetProjectByIdAsync(int projectId)
        {
            return await _projectForStudentsRepository.GetProjectByIdAsync(projectId);
        }

        public async Task<ServiceResponse<List<ProjectSubjectCountResponse>>> GetSubjectProjectCounts(ProjectForStudentRequest request)
        {
            return await _projectForStudentsRepository.GetSubjectProjectCounts(request);
        }
    }
}
