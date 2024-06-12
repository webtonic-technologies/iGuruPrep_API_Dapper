using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IFeedbackServices
    {
        Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request);
    }
}
