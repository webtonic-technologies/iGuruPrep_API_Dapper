﻿using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<ServiceResponse<int>> RegisterStudentAsync(RegistrationRequest request);
        Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request);
        Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResponse<int>> AddUpdateProfile(UpdateProfileRequest request);
        Task<ServiceResponse<RegistrationDTO>> GetRegistrationByIdAsync(int registrationId);
        Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request);
        Task<ServiceResponse<AssignStudentMappingResponse>> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request);
        Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request);
    }
}