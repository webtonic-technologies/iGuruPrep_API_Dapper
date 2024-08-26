using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;

namespace UserManagement_API.Services.Interfaces
{
    public interface IGenerateLicenseServices
    {
        Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request);
        Task<ServiceResponse<GenerateLicenseResponseDTO>> GetGenerateLicenseById(int GenerateLicenseID);
        Task<ServiceResponse<List<GenerateLicenseResponseDTO>>> GetGenerateLicenseList(GetAllLicensesListRequest request);
        Task<ServiceResponse<List<Validity>>> GetValidityList();
        Task<ServiceResponse<byte[]>> DownloadExcelFile(int GenerateLicenseID);
    }
}
