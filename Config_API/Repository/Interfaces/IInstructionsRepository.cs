using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Repository.Interfaces
{
    public interface IInstructionsRepository
    {
        Task<ServiceResponse<List<Instructions>>> GetAllInstructions(GetAllInstructionsRequest request);
        Task<ServiceResponse<Instructions>> GetInstructionById(int id);
        Task<ServiceResponse<string>> AddUpdateInstruction(Instructions request);
        Task<ServiceResponse<List<Instructions>>> GetAllInstructionsMaster();
    }
}
