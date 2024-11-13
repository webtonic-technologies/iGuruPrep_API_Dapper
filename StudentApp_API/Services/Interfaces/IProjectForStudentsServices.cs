using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
namespace StudentApp_API.Services.Interfaces
{
    public interface IProjectForStudentsServices
    {
        Task<ServiceResponse<List<ProjectForStudentsResponse>>> GetAllProjects(ProjectForStudentsRequest request);
        Task<ServiceResponse<List<ProjectSubjectCountResponse>>> GetSubjectProjectCounts(ProjectForStudentRequest request);
        Task<ServiceResponse<ProjectForStudentsResponse>> GetProjectByIdAsync(int projectId);
    }
}
