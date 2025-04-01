using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Requests;

namespace StudentApp_API.Services.Interfaces
{
    public interface IOpenChallengesServices
    {
        Task<ServiceResponse<List<CYOTResponse>>> GetOpenChallengesAsync(CYOTListRequest request);
        Task<ServiceResponse<bool>> StartChallengeAsync(int studentId, int cyotId);
    }
}