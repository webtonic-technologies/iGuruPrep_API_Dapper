using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class InstructionsServices : IInstructionsServices
    {
        private readonly IInstructionsRepository _instructionsRepository;

        public InstructionsServices(IInstructionsRepository instructionsRepository)
        {
            _instructionsRepository = instructionsRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateInstruction(Instructions request)
        {
            return await _instructionsRepository.AddUpdateInstruction(request);
        }

        public async Task<ServiceResponse<List<Instructions>>> GetAllInstructions(GetAllInstructionsRequest request)
        {
            return await _instructionsRepository.GetAllInstructions(request);
        }

        public async Task<ServiceResponse<List<Instructions>>> GetAllInstructionsMaster()
        {
            return await _instructionsRepository.GetAllInstructionsMaster();
        }

        public async Task<ServiceResponse<Instructions>> GetInstructionById(int id)
        {
            return await _instructionsRepository.GetInstructionById(id);
        }
    }
}
