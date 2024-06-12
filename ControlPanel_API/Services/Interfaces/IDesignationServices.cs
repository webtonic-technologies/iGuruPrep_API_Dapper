using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IDesignationServices
    {
        Task<ServiceResponse<List<Designation>>> GetDesignationList(GetAllDesignationsRequest request);
        Task<ServiceResponse<List<Designation>>> GetDesignationListMasters();
        Task<ServiceResponse<Designation>> GetDesignationByID(int DesgnID);
        Task<ServiceResponse<string>> AddDesignation(Designation request);
        Task<ServiceResponse<string>> UpdateDesignation(Designation request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
