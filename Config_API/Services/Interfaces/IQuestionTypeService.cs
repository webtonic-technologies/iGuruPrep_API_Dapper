using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;

namespace Config_API.Services.Interfaces
{
    public interface IQuestionTypeService
    {
        Task<ServiceResponse<string>> AddUpdateQuestionType(Questiontype request);
        Task<ServiceResponse<QuestionTypeResponse>> GetQuestionTypeByID(int Id);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypeList(GetAllQuestionTypeRequest request);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypeListMasters();
        Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList();
        Task<ServiceResponse<List<OptionType>>> OptionTypesList();
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
