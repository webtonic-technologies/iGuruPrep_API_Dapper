using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class FeedbackServices : IFeedbackServices
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackServices(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }
        public async Task<ServiceResponse<string>> AddFeedBack(Feedback request)
        {
            try
            {
                return await _feedbackRepository.AddFeedBack(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request)
        {
            try
            {
                return await _feedbackRepository.GetAllFeedBackList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllFeedbackResponse>>(false, ex.Message, new List<GetAllFeedbackResponse>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateFeedback(Feedback request)
        {
            try
            {
                return await _feedbackRepository.UpdateFeedback(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
