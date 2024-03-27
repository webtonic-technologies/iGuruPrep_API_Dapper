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
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.Now,
                        DisplayOrder = request.DisplayOrder,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = DateTime.Now,
                        Status = request.Status
                    };

                    string insertQuery = @"INSERT INTO tblCourse (CourseCode, CourseName, CreatedBy, CreatedOn, DisplayOrder, ModifiedBy, ModifiedOn, Status)
                               VALUES (@CourseCode, @CourseName, @CreatedBy, @CreatedOn, @DisplayOrder, @ModifiedBy, @ModifiedOn, @Status)";
                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newCourse);


                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Course Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var data = await _connection.QueryFirstOrDefaultAsync<Course>("SELECT * FROM tblCourse WHERE CourseId = @CourseId", new { CourseId = request.CourseId });
                    if (data != null)
                    {
                        data.CourseCode = request.CourseCode;
                        data.CourseName = request.CourseName;
                        data.CreatedBy = request.CreatedBy;
                        data.CreatedOn = request.CreatedOn;
                        data.DisplayOrder = request.DisplayOrder;
                        data.ModifiedBy = request.ModifiedBy;
                        data.ModifiedOn = DateTime.Now;
                        data.Status = request.Status;

                        string updateQuery = @"UPDATE tblCourse
                                   SET CourseCode = @CourseCode, CourseName = @CourseName, CreatedBy = @CreatedBy, CreatedOn = @CreatedOn,
                                       DisplayOrder = @DisplayOrder, ModifiedBy = @ModifiedBy, ModifiedOn = @ModifiedOn, Status = @Status
                                   WHERE CourseId = @CourseId";
                        int rowsAffected = await _connection.ExecuteAsync(updateQuery, data);
                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Course Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No Record Found", 204);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }

        }

        public async Task<ServiceResponse<List<Course>>> GetAllCourses()
        {
            try
            {
                string query = "SELECT * FROM tblCourse";
                var data = await _connection.QueryAsync<Course>(query);

                if (data != null)
                {
                    return new ServiceResponse<List<Course>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Course>>(false, "Records Not Found", new List<Course>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Course>>(false, ex.Message, new List<Course>(), 200);
            }

        }

        public async Task<ServiceResponse<Course>> GetCourseById(int id)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<Course>("SELECT * FROM tblCourse WHERE CourseId = @Id", new { Id = id });

                if (data != null)
                {
                    return new ServiceResponse<Course>(true, "Record Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<Course>(false, "Record not Found", new Course(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Course>(false, ex.Message, new Course(), 500);
            }

        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<Course>("SELECT * FROM tblCourse WHERE CourseId = @Id", new { Id = id });

                if (data != null)
                {
                    data.Status = !data.Status; // Toggle the status

                    string updateQuery = @"UPDATE tblCourse SET Status = @Status WHERE CourseId = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { Status = data.Status, Id = id });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }

        }
    }
}
