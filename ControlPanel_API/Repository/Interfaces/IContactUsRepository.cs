using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IContactUsRepository
    {
        Task<ServiceResponse<List<GetAllContactUsResponse>>> GetAllContactUs(GeAllContactUsRequest request);
    }
}
