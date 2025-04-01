using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Requests;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IOpenChallengesRepository
    {
        Task<ServiceResponse<List<CYOTResponse>>> GetOpenChallengesAsync(CYOTListRequest request);
        Task<ServiceResponse<bool>> StartChallengeAsync(int studentId, int cyotId);
    }
}
