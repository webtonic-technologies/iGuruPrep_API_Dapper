using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Repository.Interfaces
{
    public interface ITestPaperRepository
    {
        Task<ServiceResponse<int>> AddUpdateTestPaper(TestPaperRequestDTO request);
        Task<ServiceResponse<TestPaperResponseDTO>> GetTestPaperById(int TestPaperId);
        Task<ServiceResponse<string>> TestPaperContentIndexMapping(List<TestPaperContentIndex>? request, int TestPaperId);
        Task<ServiceResponse<string>> TestPaperQuestionsMapping(List<TestPaperQuestions>? request, int TestPaperId, int sectionId);
        Task<ServiceResponse<string>> TestPaperQuestionSectionMapping(List<TestPaperQuestionSection>? request, int TestPaperId);
        Task<ServiceResponse<string>> TestPaperInstructionsMapping(List<TestPaperInstructions>? request, int TestPaperId);
        Task<ServiceResponse<List<TestPaperResponseDTO>>> GetTestPaperList(TestPaperGetListRequest request);
    }
}
