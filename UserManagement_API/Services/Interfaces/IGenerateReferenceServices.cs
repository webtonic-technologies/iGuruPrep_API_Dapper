using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;

namespace UserManagement_API.Services.Interfaces
{
    public interface IGenerateReferenceServices
    {
        Task<ServiceResponse<string>> AddUpdateGenerateReference(GenerateReferenceDTO request);
        Task<ServiceResponse<GenerateReferenceDTO>> GetGenerateReferenceById(int GenerateReferenceID);
        Task<ServiceResponse<List<GenerateReferenceDTO>>> GetGenerateReferenceList();
    }
}
