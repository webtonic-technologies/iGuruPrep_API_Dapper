using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;
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

        public async Task<ServiceResponse<GenerateReferenceDTO>> GetGenerateReferenceById(int GenerateReferenceID)
        {
            try
            {
                return await _generateReferenceRepository.GetGenerateReferenceById(GenerateReferenceID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateReferenceDTO>(false, ex.Message, new GenerateReferenceDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<GenerateReferenceDTO>>> GetGenerateReferenceList()
        {
            try
            {
                return await _generateReferenceRepository.GetGenerateReferenceList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateReferenceDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}
