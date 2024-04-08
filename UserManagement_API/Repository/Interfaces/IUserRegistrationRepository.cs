using UserManagement_API.DTOs.Registration;
using UserManagement_API.DTOs.ServiceResponse;

namespace UserManagement_API.Repository.Interfaces
{
    public interface IUserRegistrationRepository
    {
        Task<ServiceResponse<string>> UserRegistration(UserRegistrationDto request);
    }
}
