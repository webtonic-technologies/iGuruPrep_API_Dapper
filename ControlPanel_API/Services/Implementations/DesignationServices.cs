﻿using ControlPanel_API.DTOs.Requests;
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
        public async Task<ServiceResponse<string>> AddUpdateDesignation(Designation request)
        {
            try
            {
                return await _designationRepository.AddUpdateDesignation(request);
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

        public async Task<ServiceResponse<List<Designation>>> GetDesignationList(GetAllDesignationsRequest request)
        {
            try
            {
                return await _designationRepository.GetDesignationList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, new List<Designation>(), 500);
            }
        }

        public async Task<ServiceResponse<List<Designation>>> GetDesignationListMasters()
        {
            try
            {
                return await _designationRepository.GetDesignationListMasters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, [], 500);
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
    }
}
