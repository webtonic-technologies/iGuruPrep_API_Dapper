using Dapper;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IDbConnection _connection;

        public CourseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<List<GetCourseResponse>>> GetAllCoursesAsync()
        {
            try
            {
                string query = @"SELECT CourseId, CourseName, CourseCode, Status
                                 FROM tblCourse
                                 WHERE Status = 1";

                var courses = await _connection.QueryAsync<GetCourseResponse>(query);

                if (courses != null)
                {
                    return new ServiceResponse<List<GetCourseResponse>>(true, "Courses retrieved successfully", courses.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<GetCourseResponse>>(false, "No courses found", new List<GetCourseResponse>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetCourseResponse>>(false, ex.Message, new List<GetCourseResponse>(), 500);
            }
        }
    }
}
