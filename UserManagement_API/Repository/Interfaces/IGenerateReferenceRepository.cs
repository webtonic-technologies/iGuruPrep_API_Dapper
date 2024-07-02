using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;

namespace UserManagement_API.Repository.Interfaces
{
    public interface IGenerateReferenceRepository
    {
        Task<ServiceResponse<string>> AddUpdateGenerateReference(GenerateReferenceDTO request);
        Task<ServiceResponse<GenerateReferenceResponseDTO>> GetGenerateReferenceById(int GenerateReferenceID);
        Task<ServiceResponse<List<GenerateReferenceResponseDTO>>> GetGenerateReferenceList(GetAllReferralsRequest request);
        Task<ServiceResponse<List<Bank>>> GetBankListMasters();
        Task<ServiceResponse<List<States>>> GetStatesListMasters();
        Task<ServiceResponse<List<Districts>>> GetDistrictsListMasters(int StateID);
    }
}
