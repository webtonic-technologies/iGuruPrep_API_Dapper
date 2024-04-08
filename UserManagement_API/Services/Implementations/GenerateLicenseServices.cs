using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Repository.Interfaces;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Services.Implementations
{
    public class GenerateLicenseServices : IGenerateLicenseServices
    {
        private readonly IGenerateLicenseRepository _generateLicenseRepository;

        public GenerateLicenseServices(IGenerateLicenseRepository generateLicenseRepository)
        {
            _generateLicenseRepository = generateLicenseRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request)
        {
            try
            {
                return await _generateLicenseRepository.AddUpdateGenerateLicense(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<GenerateLicenseDTO>> GetGenerateLicenseById(int GenerateLicenseID)
        {
            try
            {
                return await _generateLicenseRepository.GetGenerateLicenseById(GenerateLicenseID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateLicenseDTO>(false, ex.Message, new GenerateLicenseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateLicenseListDTO>>> GetGenerateLicenseList()
        {
            try
            {
                return await _generateLicenseRepository.GetGenerateLicenseList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateLicenseListDTO>>(false, ex.Message, new List<GenerateLicenseListDTO>(), 500);
            }
        }
    }
}
