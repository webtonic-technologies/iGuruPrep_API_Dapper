using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
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

        public async Task<ServiceResponse<GetAllFeedbackResponse>> GetFeedbackById(int feedbackId)
        {
            try
            {
                return await _feedbackRepository.GetFeedbackById(feedbackId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GetAllFeedbackResponse>(false, ex.Message, new GetAllFeedbackResponse(), 500);
            }
        }
    }
}
