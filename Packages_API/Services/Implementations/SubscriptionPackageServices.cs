using Packages_API.DTOs.Requests;
using Packages_API.DTOs.Response;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
using Packages_API.Repository.Interfaces;
using Packages_API.Services.Interfaces;

namespace Packages_API.Services.Implementations
{
    public class SubscriptionPackageServices : ISubscriptionPackageServices
    {

        private readonly ISubscriptionPackageRepository _subscriptionPackageRepository;

        public SubscriptionPackageServices(ISubscriptionPackageRepository subscriptionPackageRepository)
        {
            _subscriptionPackageRepository = subscriptionPackageRepository;
        }
        public async Task<ServiceResponse<bool>> AddUpdateSubscription(AddUpdateSubscriptionRequest subscription)
        {
            return await _subscriptionPackageRepository.AddUpdateSubscription(subscription);
        }

        public async Task<ServiceResponse<bool>> DeleteSubscription(int subscriptionID)
        {
            return await _subscriptionPackageRepository.DeleteSubscription(subscriptionID);
        }

        public async Task<ServiceResponse<List<CountryDTO>>> GetAllCountry()
        {
            return await _subscriptionPackageRepository.GetAllCountry();
        }

        public async Task<ServiceResponse<List<SubscriptionDTO>>> GetAllSubscriptions()
        {
            return await _subscriptionPackageRepository.GetAllSubscriptions();
        }

        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsByBoardClassCourse(SubjectRequestDTO request)
        {
            return await _subscriptionPackageRepository.GetSubjectsByBoardClassCourse(request);
        }

        public async Task<ServiceResponse<SubscriptionDTO>> GetSubscriptionByID(int subscriptionID)
        {
            return await _subscriptionPackageRepository.GetSubscriptionByID(subscriptionID);
        }

        public async Task<ServiceResponse<bool>> SubscriptionStatus(int subscriptionID)
        {
            return await _subscriptionPackageRepository.SubscriptionStatus(subscriptionID);
        }
    }
}
