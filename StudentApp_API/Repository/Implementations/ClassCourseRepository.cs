using Dapper;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Implementations
{
    public class ClassCourseRepository : IClassCourseRepository
    {
        private readonly IDbConnection _connection;

        public ClassCourseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<List<GetClassCourseResponse>>> GetClassesByCourseIdAsync(int courseId)
        {
            try
            {
                string query = @"
                    SELECT CourseClassMappingID, CourseID, ClassID, ClassName, CourseName
                    FROM tblClassCourses
                    WHERE CourseID = @CourseID AND Status = 1";

                var classes = await _connection.QueryAsync<GetClassCourseResponse>(query, new { CourseID = courseId });

                if (classes != null && classes.AsList().Count > 0)
                {
                    return new ServiceResponse<List<GetClassCourseResponse>>(true, "Classes retrieved successfully", classes.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<GetClassCourseResponse>>(false, "No classes found for this course", new List<GetClassCourseResponse>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetClassCourseResponse>>(false, ex.Message, new List<GetClassCourseResponse>(), 500);
            }
        }
    }
}
