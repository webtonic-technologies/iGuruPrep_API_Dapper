using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IDesignationRepository
    {
        Task<ServiceResponse<List<Designation>>> GetDesignationList(GetAllDesignationsRequest request);
        Task<ServiceResponse<List<Designation>>> GetDesignationListMasters();
        Task<ServiceResponse<Designation>> GetDesignationByID(int DesgnID);
        Task<ServiceResponse<string>> AddUpdateDesignation(Designation request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
