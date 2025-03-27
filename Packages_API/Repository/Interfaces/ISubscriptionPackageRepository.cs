using Packages_API.DTOs.Requests;
using Packages_API.DTOs.Response;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;

namespace Packages_API.Repository.Interfaces
{
    public interface ISubscriptionPackageRepository
    {
        Task<ServiceResponse<bool>> AddUpdateSubscription(AddUpdateSubscriptionRequest subscription);
        Task<ServiceResponse<List<SubscriptionDTO>>> GetAllSubscriptions();
        Task<ServiceResponse<List<CountryDTO>>> GetAllCountry();
        Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsByBoardClassCourse(SubjectRequestDTO request);
        Task<ServiceResponse<SubscriptionDTO>> GetSubscriptionByID(int subscriptionID);
        Task<ServiceResponse<bool>> DeleteSubscription(int subscriptionID);
        Task<ServiceResponse<bool>> SubscriptionStatus(int subscriptionID);
    }
}
