using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request);
        Task<ServiceResponse<GetAllFeedbackResponse>> GetFeedbackById(int feedbackId);
    }
}
