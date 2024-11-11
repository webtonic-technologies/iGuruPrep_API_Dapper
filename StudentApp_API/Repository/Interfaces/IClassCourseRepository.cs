using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Interfaces
{
    public interface IClassCourseRepository
    {
        Task<ServiceResponse<List<GetClassCourseResponse>>> GetClassesByCourseIdAsync(int courseId);
    }
}
