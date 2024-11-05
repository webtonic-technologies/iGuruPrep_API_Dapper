using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Services.Interfaces
{
    public interface IScholarshipTestServices
    {
        Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request);
        Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId);
        Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request);
        Task<ServiceResponse<string>> ScholarshipContentIndexMapping(ContentIndexRequest request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId);
        Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<QuestionSectionScholarship> request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipInstructionsMapping(ScholarshipTestInstructions? request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId);
    }
}
