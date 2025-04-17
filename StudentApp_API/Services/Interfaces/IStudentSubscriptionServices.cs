using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;

namespace StudentApp_API.Services.Interfaces
{
    public interface IStudentSubscriptionServices
    {
        Task<ServiceResponse<List<SubscriptionResponseDTO>>> GetSubscriptionPackages(SubscriptionRequestDTO request);
        Task<ServiceResponse<string>> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request);
    }
}
