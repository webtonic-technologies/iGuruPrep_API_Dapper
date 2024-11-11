using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<ServiceResponse<int>> AddRegistrationAsync(RegistrationRequest request);
        Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request);
        Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);

        Task<ServiceResponse<AssignCourseResponse>> AssignCourseAsync(AssignCourseRequest request);

        Task<ServiceResponse<AssignClassResponse>> AssignClassAsync(AssignClassRequest request);


    }
}
