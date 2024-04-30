using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request)
        {
            try
            {
                var classData = await _connection.QueryAsync<Class>("SELECT * FROM tblClass WHERE ClassId = @ClassId", new { ClassId = request.ClassID });
                //var courseData = await _connection.QueryAsync<Course>("SELECT * FROM tblCourse WHERE CourseId = @CourseId", new { CourseId = request.CourseID });
                if (classData != null)
                {
                    if (request.CourseClassMappingID == 0)
                    {

                        foreach (var courseId in request.CourseID ??= ([]))
                        {
                            string query = @"INSERT INTO tblClassCourses (CourseID, ClassID, Status, createdon, EmployeeID) 
                                 VALUES (@CourseID, @ClassID, @Status, @createdon, @EmployeeID)";

                            int rowsAffected = await _connection.ExecuteAsync(query, new
                            {
                                CourseID = courseId,
                                request.ClassID,
                                Status = true,
                                createdon = DateTime.Now,
                                request.EmployeeID
                            });
                            if (rowsAffected == 0)
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                            }
                        }
                        return new ServiceResponse<string>(true, "Operation Successful", "Record added successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        string delete = "DELETE FROM tblClassCourses WHERE ClassID = @ClassID";
                        int deletedRows = await _connection.ExecuteAsync(delete, new { request.ClassID });

                        foreach (var courseId in request.CourseID ??= ([]))
                        {
                            //string query = @"UPDATE tblClassCourses 
                            //     SET ClassID = @ClassID, 
                            //         Status = @Status,
                            //         modifiedon = @modifiedon,
                            //         modifiedby = @modifiedby
                            //     WHERE CourseClassMappingID = @CourseClassMappingID";
                            string query = @"INSERT INTO tblClassCourses (CourseID, ClassID, Status, createdon, EmployeeID, modifiedon, modifiedby) 
                                 VALUES (@CourseID, @ClassID, @Status, @createdon, @EmployeeID, @modifiedon, @modifiedby)";
                            int rowsAffected = await _connection.ExecuteAsync(query, new
                            {
                                request.ClassID,
                                request.Status,
                                createdon = DateTime.Now,
                                modifiedon = DateTime.UtcNow,
                                request.modifiedby,
                                CourseID = courseId,
                                request.CourseClassMappingID,
                                request.EmployeeID
                            });
                            if (rowsAffected == 0)
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                            }
                        }
                        return new ServiceResponse<string>(true, "Operation Successful", "Record Updated successfully", StatusCodes.Status200OK);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "No record found for class or course", 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<ClassCourseMappingDTO>>> GetAllClassCoursesMappings()
        {
            try
            {
                string query = "SELECT CourseClassMappingID, ClassID, CourseID, Status, createdon, EmployeeID, modifiedon, modifiedby FROM [tblClassCourses]";
                var classCourseMappings = await _connection.QueryAsync<ClassCourseMapping>(query);

                var groupedMappings = classCourseMappings
                .GroupBy(m => m.ClassID)
                .Select(g => new ClassCourseMappingDTO
                {
                    ClassID = g.Key,
                    CourseID = g.Select(m => m.CourseID).ToList(),
                    Status = g.First().Status,
                    createdon = g.First().createdon,
                    EmployeeID = g.First().EmployeeID,
                    modifiedon = g.First().modifiedon,
                    modifiedby = g.First().modifiedby,
                    CourseClassMappingID = g.First().CourseClassMappingID
                }).ToList();
                if (groupedMappings != null)
                {
                    return new ServiceResponse<List<ClassCourseMappingDTO>>(true, "Records Found", groupedMappings, 200);
                }
                else
                {
                    return new ServiceResponse<List<ClassCourseMappingDTO>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMappingDTO>>(false, ex.Message, [], 200);
            }

        }

        public async Task<ServiceResponse<ClassCourseMappingDTO>> GetClassCourseMappingById(int id)
        {
            try
            {
                var response = new ClassCourseMappingDTO();
                string getClassIdQuery = @"
                SELECT CourseClassMappingID, ClassID, Status, createdon, EmployeeID, modifiedon, modifiedby
                FROM [tblClassCourses]
                WHERE CourseClassMappingID = @CourseClassMappingID";

                var classId = await _connection.QueryFirstOrDefaultAsync<ClassCourseMapping?>(getClassIdQuery, new { CourseClassMappingID = id });
                if (classId == null)
                {
                    // Handle the case where no class ID is found for the given courseClassMappingId
                     return new ServiceResponse<ClassCourseMappingDTO>(false, "Record not Found", new ClassCourseMappingDTO(), 500);
                }
                string query = @"
                SELECT 
                    CourseClassMappingID, 
                    CourseID, 
                    ClassID, 
                    Status, 
                    createdon, 
                    EmployeeID, 
                    modifiedon, 
                    modifiedby 
                FROM 
                    [tblClassCourses]
                WHERE 
                    ClassID = @ClassID";
                var data = await _connection.QueryAsync<ClassCourseMapping>(query, new { classId.ClassID });
                List<int> courses = [];

                foreach(var item in data)
                {
                    courses.Add((int)item.CourseID);
                }
                response.CourseID = courses;
                response.CourseClassMappingID = classId.CourseClassMappingID;
                response.ClassID = classId.ClassID;
                response.Status = classId.Status;
                response.createdon = classId.createdon;
                response.EmployeeID = classId.EmployeeID;
                response.modifiedby = classId.modifiedby;
                response.modifiedon = classId.modifiedon;

                if (response != null)
                {
                    return new ServiceResponse<ClassCourseMappingDTO>(true, "Record Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<ClassCourseMappingDTO>(false, "Record not Found", new ClassCourseMappingDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ClassCourseMappingDTO>(false, ex.Message, new ClassCourseMappingDTO(), 500);
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