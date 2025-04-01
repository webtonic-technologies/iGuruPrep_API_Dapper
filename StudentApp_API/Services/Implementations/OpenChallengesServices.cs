using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Implementations;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class OpenChallengesServices : IOpenChallengesServices
    {
        private readonly IOpenChallengesRepository _openChallengesRepository;

        public OpenChallengesServices(IOpenChallengesRepository openChallengesRepository)
        {
            _openChallengesRepository = openChallengesRepository;
        }
        public async Task<ServiceResponse<List<CYOTResponse>>> GetOpenChallengesAsync(CYOTListRequest request)
        {
            return await _openChallengesRepository.GetOpenChallengesAsync(request);
        }

        public async Task<ServiceResponse<bool>> StartChallengeAsync(int studentId, int cyotId)
        {
            return await _openChallengesRepository.StartChallengeAsync(studentId, cyotId);
        }
    }
}
