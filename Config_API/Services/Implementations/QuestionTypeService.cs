using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Implementations;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class QuestionTypeService : IQuestionTypeService
    {
        private readonly IQuestionTypeRepository _questionTypeRepository;

        public QuestionTypeService(IQuestionTypeRepository questionTypeRepository)
        {
            _questionTypeRepository = questionTypeRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionType(Questiontype request)
        {
            try
            {
                return await _questionTypeRepository.AddUpdateQuestionType(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<Questiontype>> GetQuestionTypeByID(int Id)
        {
            try
            {
                return await _questionTypeRepository.GetQuestionTypeByID(Id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Questiontype>(false, ex.Message, new Questiontype(), 500);
            }
        }

        public async Task<ServiceResponse<List<Questiontype>>> GetQuestionTypeList()
        {
            try
            {
                return await _questionTypeRepository.GetQuestionTypeList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Questiontype>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList()
        {
            try
            {
                return await _questionTypeRepository.NoOfOptionsList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NoOfOptions>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<OptionType>>> OptionTypesList()
        {
            try
            {
                return await _questionTypeRepository.OptionTypesList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<OptionType>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _questionTypeRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
