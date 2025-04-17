using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class StudentSubscriptionServices : IStudentSubscriptionServices
    {
        private readonly IStudentSubscriptionRepository _studentSubscriptionRepository;

        public StudentSubscriptionServices(IStudentSubscriptionRepository studentSubscriptionRepository)
        {
            _studentSubscriptionRepository = studentSubscriptionRepository;
        }
        public async Task<ServiceResponse<List<SubscriptionResponseDTO>>> GetSubscriptionPackages(SubscriptionRequestDTO request)
        {
            return await _studentSubscriptionRepository.GetSubscriptionPackages(request);
        }

        public async Task<ServiceResponse<string>> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request)
        {
            return await _studentSubscriptionRepository.InsertStudentSubscriptionAsync(request);
        }
    }
}
