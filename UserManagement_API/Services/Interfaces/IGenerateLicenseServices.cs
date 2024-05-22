using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;

namespace UserManagement_API.Services.Interfaces
{
    public interface IGenerateLicenseServices
    {
        Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request);
        Task<ServiceResponse<GenerateLicenseDTO>> GetGenerateLicenseById(int GenerateLicenseID);
        Task<ServiceResponse<List<GenerateLicenseDTO>>> GetGenerateLicenseList();
    }
}
