using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IQuestionTypeService
    {
        Task<ServiceResponse<string>> AddUpdateQuestionType(Questiontype request);
        Task<ServiceResponse<Questiontype>> GetQuestionTypeByID(int Id);
        Task<ServiceResponse<List<Questiontype>>> GetQuestionTypeList(GetAllQuestionTypeRequest request);
        Task<ServiceResponse<List<Questiontype>>> GetQuestionTypeListMasters();
        Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList();
        Task<ServiceResponse<List<OptionType>>> OptionTypesList();
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
