using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;

namespace UserManagement_API.Repository.Interfaces
{
    public interface IGenerateLicenseRepository
    {
        Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request);
        Task<ServiceResponse<GenerateLicenseDTO>> GetGenerateLicenseById(int GenerateLicenseID);
        Task<ServiceResponse<List<GenerateLicenseListDTO>>> GetGenerateLicenseList();
    }
}
