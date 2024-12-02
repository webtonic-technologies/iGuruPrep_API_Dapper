using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Threading.Tasks;
using static StudentApp_API.Repository.Implementations.ScholarshipRepository;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IScholarshipRepository
    {
        Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request);
        Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request);
        Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(int scholarshipTestId);
        Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId);
        Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId);
        Task<ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>> SubmitAnswer(List<AnswerSubmissionRequest> request);
    }
}
