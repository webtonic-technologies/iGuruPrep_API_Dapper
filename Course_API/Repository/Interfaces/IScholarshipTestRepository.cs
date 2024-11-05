using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Repository.Interfaces
{
    public interface IScholarshipTestRepository
    {
        Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request);
        Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId);
        Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request);
        Task<ServiceResponse<string>> ScholarshipContentIndexMapping(List<ScholarshipContentIndex> request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId);
        Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipInstructionsMapping(ScholarshipTestInstructions? request, int ScholarshipTestId);
        Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId);
        //Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request);
        //we have an API to fetch list of questions depending on subjectid in Test series module
    }
}