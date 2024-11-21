using Dapper;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

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

        public async Task<ServiceResponse<List<GetBoardsResponse>>> GetBoardsAsync()
        {

            try
            {
                string sql = @"SELECT [BoardId]
                                  ,[BoardName]
                                  ,[BoardCode]
                                  ,[Status]
                                  ,[showcourse]
                                  ,[createdon]
                                  ,[createdby]
                                  ,[modifiedon]
                                  ,[modifiedby]
                                  ,[EmployeeID]
                                  ,[EmpFirstName]
                            FROM tblBoard where Status = 1";

                var boards = await _connection.QueryAsync<GetBoardsResponse>(sql);
                if (boards.Any())
                {
                    return new ServiceResponse<List<GetBoardsResponse>>(true, "Records Found", boards.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<GetBoardsResponse>>(false, "Records Not Found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetBoardsResponse>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<List<GetClassesResponse>>> GetClassesAsync()
        {
            try
            {
                string query = @"SELECT [ClassId]
                                  ,[ClassName]
                                  ,[ClassCode]
                                  ,[Status]
                                  ,[createdby]
                                  ,[createdon]
                                  ,[modifiedby]
                                  ,[modifiedon]
                                  ,[showcourse]
                                  ,[EmployeeID]
                                  ,[EmpFirstName]
                           FROM [tblClass] where Status = 1";
                var classes = await _connection.QueryAsync<GetClassesResponse>(query);

                if (classes.Any())
                {
                    return new ServiceResponse<List<GetClassesResponse>>(true, "Records Found", classes.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<GetClassesResponse>>(false, "Records Not Found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetClassesResponse>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
    }
}
