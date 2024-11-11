using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Interfaces
{
    public interface IScholarshipService
    {
        Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request);
        Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request);
        Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request);

    }
}
