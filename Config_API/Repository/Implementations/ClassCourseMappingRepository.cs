using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class ClassCourseMappingRepository : IClassCourseMappingRepository
    {
        private readonly IDbConnection _connection;

        public ClassCourseMappingRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMapping request)
        {
            try
            {
                if (request.CourseClassMappingID == 0)
                {
                    var classData = await _connection.QueryFirstOrDefaultAsync<Class>("SELECT * FROM tblClass WHERE ClassId = @ClassId", new { ClassId = request.ClassID });
                    var courseData = await _connection.QueryFirstOrDefaultAsync<Course>("SELECT * FROM tblCourse WHERE CourseId = @CourseId", new { CourseId = request.CourseID });

                    if (classData != null && courseData != null)
                    {
                        var newClassCourseMapping = new ClassCourseMapping
                        {
                            ClassID = request.ClassID,
                            CourseID = request.CourseID,
                            CreatedOn = DateTime.Now,
                            Status = request.Status
                        };

                        string insertQuery = @"INSERT INTO tblClassCourses (ClassID, CourseID, CreatedOn, Status)
                                   VALUES (@ClassID, @CourseID, @CreatedOn, @Status)";
                        int rowsAffected = await _connection.ExecuteAsync(insertQuery, newClassCourseMapping);

                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Class Course Mapping Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No record found for class or course", 204);
                    }
                }
                else
                {
                    var data = await _connection.QueryFirstOrDefaultAsync<ClassCourseMapping>("SELECT * FROM tblClassCourses WHERE CourseClassMappingID = @CourseClassMappingID", new { CourseClassMappingID = request.CourseClassMappingID });
                    if (data != null)
                    {
                        data.CreatedOn = request.CreatedOn;
                        data.Status = request.Status;
                        data.ClassID = request.ClassID;
                        data.CourseID = request.CourseID;

                        string updateQuery = @"UPDATE tblClassCourses
                                   SET ClassID = @ClassID, CourseID = @CourseID, CreatedOn = @CreatedOn, Status = @Status
                                   WHERE CourseClassMappingID = @CourseClassMappingID";
                        int rowsAffected = await _connection.ExecuteAsync(updateQuery, data);


                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Class Course Mapping Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "No record found", 204);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }

        }

        public async Task<ServiceResponse<List<ClassCourseMapping>>> GetAllClassCoursesMappings()
        {
            try
            {
                string query = "SELECT * FROM tblClassCourses";

                var data = await _connection.QueryAsync<ClassCourseMapping>(query);

                if (data != null)
                {
                    return new ServiceResponse<List<ClassCourseMapping>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<ClassCourseMapping>>(false, "Records Not Found", new List<ClassCourseMapping>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMapping>>(false, ex.Message, new List<ClassCourseMapping>(), 200);
            }

        }

        public async Task<ServiceResponse<ClassCourseMapping>> GetClassCourseMappingById(int id)
        {
            try
            {
                string query = "SELECT * FROM tblClassCourses WHERE CourseClassMappingID = @Id";
                var data = await _connection.QueryFirstOrDefaultAsync<ClassCourseMapping>(query, new { Id = id });

                if (data != null)
                {
                    return new ServiceResponse<ClassCourseMapping>(true, "Record Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<ClassCourseMapping>(false, "Record not Found", new ClassCourseMapping(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ClassCourseMapping>(false, ex.Message, new ClassCourseMapping(), 500);
            }

        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<ClassCourseMapping>("SELECT * FROM tblClassCourses WHERE CourseClassMappingID = @Id", new { Id = id });

                if (data != null)
                {
                    // Toggle the status
                    data.Status = !data.Status;

                    string updateQuery = "UPDATE tblClassCourses SET Status = @Status WHERE CourseClassMappingID = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { data.Status, Id = id });

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
