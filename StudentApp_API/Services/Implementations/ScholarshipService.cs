using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;
using System.Threading.Tasks;

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

        public async Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request)
        {
            return await _scholarshipRepository.GetScholarshipTestAsync(request);
        }

        public async Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request)
        {
            return await _scholarshipRepository.UpdateQuestionNavigationAsync(request);
        }
    }
}
