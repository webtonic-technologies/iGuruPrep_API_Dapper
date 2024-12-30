using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class ScholarshipService : IScholarshipService
    {
        private readonly IScholarshipRepository _scholarshipRepository;

        public ScholarshipService(IScholarshipRepository scholarshipRepository)
        {
            _scholarshipRepository = scholarshipRepository;
        }

        public async Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request)
        {
            return await _scholarshipRepository.AssignScholarshipAsync(request);
        }

        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(int scholarshipTestId, int studentId)
        {
            return await _scholarshipRepository.GetQuestionsBySectionSettings(scholarshipTestId, studentId);
        }

        public async Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId)
        {
            return await _scholarshipRepository.GetScholarshipSubjectQuestionCount(scholarshipTestId);
        }

        public async Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request)
        {
            return await _scholarshipRepository.GetScholarshipTestAsync(request);
        }

        public async Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId)
        {
            return await _scholarshipRepository.GetScholarshipTestByRegistrationId(registrationId);
        }

        public async Task<ServiceResponse<string>> MarkScholarshipQuestionAsSave(ScholarshipQuestionSaveRequest request)
        {
            return await _scholarshipRepository.MarkScholarshipQuestionAsSave(request);
        }

        public async Task<ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>> SubmitAnswer(List<AnswerSubmissionRequest> request)
        {
            return await _scholarshipRepository.SubmitAnswer(request);
        }

        public async Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request)
        {
            return await _scholarshipRepository.UpdateQuestionNavigationAsync(request);
        }
    }
}
