using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class ScholarshipTestServices: IScholarshipTestServices
    {
        private readonly IScholarshipTestRepository _scholarshipTestRepository;

        public ScholarshipTestServices(IScholarshipTestRepository scholarshipTestRepository)
        {
            _scholarshipTestRepository = scholarshipTestRepository;
        }

        public async Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request)
        {
            try
            {
                return await _scholarshipTestRepository.AddUpdateScholarshipTest(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }

        public async Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId)
        {

            try
            {
                return await _scholarshipTestRepository.GetScholarshipTestById(ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipTestResponseDTO>(false, ex.Message,  new ScholarshipTestResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request)
        {
            try
            {
                return await _scholarshipTestRepository.GetScholarshipTestList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipContentIndexMapping(List<ScholarshipContentIndex> request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipContentIndexMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipDiscountSchemeMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipInstructionsMapping(List<ScholarshipTestInstructions>? request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipInstructionsMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipQuestionSectionMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipQuestionsMapping(request, ScholarshipTestId, SSTSectionId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
