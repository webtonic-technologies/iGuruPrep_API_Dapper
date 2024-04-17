using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Implementations;
using Schools_API.Repository.Interfaces;
using Schools_API.Services.Interfaces;

namespace Schools_API.Services.Implementations
{
    public class QuestionServices : IQuestionServices
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionServices(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }
        public async Task<ServiceResponse<string>> AddQuestion(QuestionDTO request)
        {
            try
            {
                return await _questionRepository.AddQuestion(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Question>>> GetAllQuestionsList()
        {
            try
            {
                return await _questionRepository.GetAllQuestionsList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Question>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId)
        {
            try
            {
                return await _questionRepository.GetQuestionById(questionId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionDTO>(false, ex.Message, new QuestionDTO(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateQuestionImageFile(QuestionImageDTO request)
        {
            try
            {
                return await _questionRepository.UpdateQuestionImageFile(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
