﻿using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<ServiceResponse<List<Employee>>> GetEmployeeList(GetEmployeeListDTO request);
        Task<ServiceResponse<Employee>> GetEmployeeByID(int ID);
        Task<ServiceResponse<string>> AddUpdateEmployee(Employee request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}