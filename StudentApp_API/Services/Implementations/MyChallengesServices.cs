using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class MyChallengesServices: IMyChallengesServices
    {
        private readonly IMyChallengesRepository _myChallengesRepository;

        public MyChallengesServices(IMyChallengesRepository myChallengesRepository)
        {
            _myChallengesRepository = myChallengesRepository;
        }

        public async Task<ServiceResponse<string>> DeleteCYOT(int CYOTId)
        {
            return await _myChallengesRepository.DeleteCYOT(CYOTId);
        }

        public async Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            return await _myChallengesRepository.GetCYOTAnalyticsAsync(studentId, cyotId);
        }

        public async Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request)
        {
            return await _myChallengesRepository.GetCYOTListByStudent(request);
        }

        public async Task<ServiceResponse<MarksComparison>> GetCYOTMarksComparisonAsync(int studentId, int cyotId)
        {
            return await _myChallengesRepository.GetCYOTMarksComparisonAsync(studentId, cyotId);
        }

        public async Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTSubjectWiseAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            return await _myChallengesRepository.GetCYOTSubjectWiseAnalyticsAsync(studentId, cyotId, subjectId);
        }

        public async Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTSubjectWiseTimeAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            return await _myChallengesRepository.GetCYOTSubjectWiseTimeAnalyticsAsync(studentId, cyotId, subjectId);
        }

        public async Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            return await _myChallengesRepository.GetCYOTTimeAnalyticsAsync(studentId, cyotId);
        }

        public async Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId, int studentId)
        {
            return await _myChallengesRepository.MakeCYOTOpenChallenge(CYOTId, studentId);
        }
    }
}
