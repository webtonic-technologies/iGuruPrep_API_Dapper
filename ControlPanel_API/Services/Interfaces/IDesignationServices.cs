using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IDesignationServices
    {
        Task<ServiceResponse<List<Designation>>> GetDesignationList();
        Task<ServiceResponse<Designation>> GetDesignationByID(int DesgnID);
        Task<ServiceResponse<string>> AddDesignation(Designation request);
        Task<ServiceResponse<string>> UpdateDesignation(Designation request);
    }
}
