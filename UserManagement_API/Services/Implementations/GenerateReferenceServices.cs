using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;
using UserManagement_API.Services.Interfaces;

namespace UserManagement_API.Services.Implementations
{
    public class GenerateReferenceServices : IGenerateReferenceServices
    {
        private readonly IGenerateReferenceRepository _generateReferenceRepository;

        public GenerateReferenceServices(IGenerateReferenceRepository generateReferenceRepository)
        {
            _generateReferenceRepository = generateReferenceRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateGenerateReference(GenerateReferenceDTO request)
        {
            try
            {
                return await _generateReferenceRepository.AddUpdateGenerateReference(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Bank>>> GetBankListMasters()
        {
            try
            {
                return await _generateReferenceRepository.GetBankListMasters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Bank>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<Districts>>> GetDistrictsListMasters(int StateID)
        {
            try
            {
                return await _generateReferenceRepository.GetDistrictsListMasters(StateID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Districts>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<GenerateReferenceResponseDTO>> GetGenerateReferenceById(int GenerateReferenceID)
        {
            try
            {
                return await _generateReferenceRepository.GetGenerateReferenceById(GenerateReferenceID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateReferenceResponseDTO>(false, ex.Message, new GenerateReferenceResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<GenerateReferenceResponseDTO>>> GetGenerateReferenceList(GetAllReferralsRequest request)
        {
            try
            {
                return await _generateReferenceRepository.GetGenerateReferenceList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateReferenceResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<States>>> GetStatesListMasters()
        {
            try
            {
                return await _generateReferenceRepository.GetStatesListMasters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<States>>(false, ex.Message, [], 500);
            }
        }
    }
}
