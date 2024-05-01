using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Repository.Interfaces
{
    public interface IQuestionTypeRepository
    {
        Task<ServiceResponse<string>> AddUpdateQuestionType(Questiontype request);
        Task<ServiceResponse<Questiontype>> GetQuestionTypeByID(int Id);
        Task<ServiceResponse<List<Questiontype>>> GetQuestionTypeList();
        Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList();
        Task<ServiceResponse<List<OptionType>>> OptionTypesList();
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
