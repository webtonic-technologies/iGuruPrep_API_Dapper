using Config_API.DTOs.ServiceResponse;
using UserManagement_API.DTOs.Registration;

namespace UserManagement_API.Repository.Interfaces
{
    public interface IUserRegistrationRepository
    {
        Task<ServiceResponse<string>> UserRegistration(UserRegistrationDto request);
    }
}
