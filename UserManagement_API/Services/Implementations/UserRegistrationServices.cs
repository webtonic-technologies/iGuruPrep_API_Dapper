using UserManagement_API.DTOs.Registration;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Repository.Interfaces;

namespace UserManagement_API.Services.Implementations
{
    public class UserRegistrationServices : IUserRegistrationServices
    {
        private readonly IUserRegistrationRepository _userRegistrationRepository;

        public UserRegistrationServices(IUserRegistrationRepository userRegistrationRepository)
        {
            _userRegistrationRepository = userRegistrationRepository;
        }
        public async Task<ServiceResponse<string>> UserRegistration(UserRegistrationDto request)
        {
            try
            {
                return await _userRegistrationRepository.UserRegistration(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
