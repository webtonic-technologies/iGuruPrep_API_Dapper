using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IScholarshipRepository
    {
        Task<ServiceResponse<List<QuestionResponseDTO>>> AssignScholarshipAsync(AssignScholarshipRequest request);
        Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request);
        Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(GetScholarshipQuestionRequest request);
        Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId);
        Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId);
        Task<ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>> SubmitAnswer(List<AnswerSubmissionRequest> request);
        Task<ServiceResponse<string>> MarkScholarshipQuestionAsSave(ScholarshipQuestionSaveRequest request);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByScholarshipId(int scholarshipId);
        Task<ServiceResponse<List<QuestionResponseDTO>>> ViewKeyByStudentScholarship(GetScholarshipQuestionRequest request);
        Task<ServiceResponse<StudentDiscountResponse>> GetStudentDiscountAsync(int studentId, int scholarshipTestId);
        Task<ServiceResponse<int>> AddReviewAsync(int scholarshipId, int studentId, int questionId);
        Task<ServiceResponse<TimeSpentReport>> GetSubjectWiseTimeSpentReportAsync(int studentId, int scholarshipId, int subjectId);
        Task<ServiceResponse<TimeSpentReport>> GetTimeSpentReportAsync(int studentId, int scholarshipId);
        Task<ServiceResponse<MarksCalculation>> GetSubjectWiseMarksCalculationAsync(int studentId, int scholarshipId, int subjectId);
        Task<ServiceResponse<MarksCalculation>> GetMarksCalculationAsync(int studentId, int scholarshipId);
        Task<ServiceResponse<ScholarshipAnalytics>> GetSubjectWiseScholarshipAnalyticsAsync(int studentId, int scholarshipId, int subjectId);
        Task<ServiceResponse<ScholarshipAnalytics>> GetScholarshipAnalyticsAsync(int studentId, int scholarshipId);
        Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int ScholarshipID, int studentId, int questionId, bool isAnswered);
    }
}
