using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IProjectForStudentsRepository
    {
        Task<ServiceResponse<List<ProjectForStudentsResponse>>> GetAllProjects(ProjectForStudentsRequest request);
        Task<ServiceResponse<List<ProjectSubjectCountResponse>>> GetSubjectProjectCounts(ProjectForStudentRequest request);
        Task<ServiceResponse<ProjectForStudentsResponse>> GetProjectByIdAsync(int projectId);
    }
}
