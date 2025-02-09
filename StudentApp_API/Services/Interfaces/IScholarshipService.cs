using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;

namespace StudentApp_API.Services.Interfaces
{
    public interface IScholarshipService
    {
        Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request);
        Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request);
        Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(GetScholarshipQuestionRequest request);
        Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId);
        Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId);
        Task<ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>> SubmitAnswer(List<AnswerSubmissionRequest> request);
        Task<ServiceResponse<string>> MarkScholarshipQuestionAsSave(ScholarshipQuestionSaveRequest request);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByScholarshipId(int scholarshipId);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsByStudentScholarship(GetScholarshipQuestionRequest request);
        Task<ServiceResponse<StudentDiscountResponse>> GetStudentDiscountAsync(int studentId, int scholarshipTestId);
    }
}
