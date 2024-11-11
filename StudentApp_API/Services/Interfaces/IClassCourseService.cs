using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentApp_API.Services.Interfaces
{
    public interface IClassCourseService
    {
        Task<ServiceResponse<List<GetClassCourseResponse>>> GetClassesByCourseIdAsync(int courseId);
    }
}
