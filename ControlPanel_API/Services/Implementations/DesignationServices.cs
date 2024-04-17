using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class DesignationServices : IDesignationServices
    {
        private readonly IDesignationRepository _designationRepository;


        public DesignationServices(IDesignationRepository designationRepository)
        {
            _designationRepository = designationRepository;
        }
        public async Task<ServiceResponse<string>> AddDesignation(Designation request)
        {
            try
            {
                return await _designationRepository.AddDesignation(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<Designation>> GetDesignationByID(int DesgnID)
        {
            try
            {
                return await _designationRepository.GetDesignationByID(DesgnID);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Designation>(false, ex.Message, new Designation(), 500);
            }
        }

        public async Task<ServiceResponse<List<Designation>>> GetDesignationList()
        {
            try
            {
                return await _designationRepository.GetDesignationList();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, new List<Designation>(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _designationRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateDesignation(Designation request)
        {
            try
            {
                return await _designationRepository.UpdateDesignation(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
