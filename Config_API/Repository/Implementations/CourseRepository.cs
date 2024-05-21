using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class CourseRepository : ICourseRepository
    {

        private readonly IDbConnection _connection;

        public CourseRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateCourse(Course request)
        {
            try
            {
                if (request.CourseId == 0)
                {
                    var newCourse = new Course
                    {
                        CourseCode = request.CourseCode,
                        CourseName = request.CourseName,
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        displayorder = request.displayorder,
                        EmployeeID = request.EmployeeID,
                        EmpFirstName = request.EmpFirstName,
                        Status = true
                    };

                    string insertQuery = @"INSERT INTO [tblCourse] 
                           ([CourseName], [CourseCode], [Status], [createdby], [createdon], [displayorder], [EmployeeID], [EmpFirstName])
                           VALUES (@CourseName, @CourseCode, @Status, @CreatedBy, GETDATE(), @DisplayOrder, @EmployeeID, @EmpFirstName)";
                    
                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newCourse);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Course Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE [tblCourse] SET 
                           [CourseName] = @CourseName, 
                           [CourseCode] = @CourseCode, 
                           [Status] = @Status, 
                           [displayorder] = @DisplayOrder, 
                           [modifiedby] = @ModifiedBy, 
                           [modifiedon] = GETDATE(), 
                           [EmployeeID] = @EmployeeID,
                          [EmpFirstName] = @EmpFirstName
                           WHERE [CourseId] = @CourseId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.CourseName,
                        request.CourseCode,
                        request.Status,
                        request.displayorder,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.CourseId
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Course Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<ServiceResponse<List<Course>>> GetAllCourses()
        {
            try
            {
                string query = @"SELECT [CourseId], [CourseName], [CourseCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [EmployeeID], [EmpFirstName]
                           FROM [tblCourse]";
                var data = await _connection.QueryAsync<Course>(query);

                if (data != null)
                {
                    return new ServiceResponse<List<Course>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Course>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Course>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<ServiceResponse<Course>> GetCourseById(int id)
        {
            try
            {
                string getQuery = @"SELECT [CourseId], [CourseName], [CourseCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [EmployeeID], [EmpFirstName]
                           FROM [tblCourse]
                           WHERE [CourseId] = @CourseId";
                var data = await _connection.QueryFirstOrDefaultAsync<Course>(getQuery, new { CourseId = id });

                if (data != null)
                {
                    return new ServiceResponse<Course>(true, "Record Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Course>(false, "Record not Found", new Course(), StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Course>(false, ex.Message, new Course(), StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetCourseById(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status; // Toggle the status

                    string updateQuery = @"UPDATE tblCourse SET Status = @Status WHERE CourseId = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { data.Data.Status, Id = id });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, StatusCodes.Status304NotModified);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, StatusCodes.Status500InternalServerError);
            }

        }
    }
}
