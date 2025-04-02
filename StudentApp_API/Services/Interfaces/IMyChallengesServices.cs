using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Requests;

namespace StudentApp_API.Services.Interfaces
{
    public interface IMyChallengesServices
    {
        Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request);
        Task<ServiceResponse<string>> DeleteCYOT(int CYOTId);
        Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId, int studentId);
        Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId);
        Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId);
        Task<ServiceResponse<MarksComparison>> GetCYOTMarksComparisonAsync(int studentId, int cyotId);
        Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTSubjectWiseAnalyticsAsync(int studentId, int cyotId, int subjectId);
        Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTSubjectWiseTimeAnalyticsAsync(int studentId, int cyotId, int subjectId);
        Task<ServiceResponse<List<LeaderboardResponse>>> GetCYOTLeaderboardAsync(int cyotId, int studentId);
    }
}
