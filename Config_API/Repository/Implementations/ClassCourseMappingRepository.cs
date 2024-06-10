using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
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
        public async Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblClassCourses]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                string query = @"
        SELECT 
            cc.CourseClassMappingID,
            cc.CourseID,
            cc.ClassID,
            cc.Status,
            cc.createdon,
            cc.EmployeeID,
            e.EmpFirstName,
            cc.modifiedon,
            cc.modifiedby,
            cl.classname,
            c.coursename
        FROM 
            tblClassCourses cc
        INNER JOIN tblClasses cl ON cc.ClassID = cl.ClassID
        INNER JOIN tblCourses c ON cc.CourseID = c.CourseID
        LEFT JOIN tblEmployees e ON cc.EmployeeID = e.EmployeeID";

                var classCourseMappings = await _connection.QueryAsync<dynamic>(query);

                var groupedMappings = classCourseMappings
                    .GroupBy(m => m.ClassID)
                    .Select(g => new ClassCourseMappingResponse
                    {
                        CourseClassMappingID = g.First().CourseClassMappingID,
                        ClassID = g.Key,
                        Status = g.First().Status,
                        createdon = g.First().createdon,
                        EmployeeID = g.First().EmployeeID,
                        modifiedon = g.First().modifiedon,
                        modifiedby = g.First().modifiedby,
                        classname = g.First().classname,
                        Courses = g.Select(m => new CourseData
                        {
                            CourseID = m.CourseID,
                            Coursename = m.coursename
                        }).ToList()
                    }).ToList();

                var paginatedList = groupedMappings
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<ClassCourseMappingResponse>>(true, "Records Found", paginatedList, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ClassCourseMappingResponse>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMappingResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<ClassCourseMappingResponse>> GetClassCourseMappingById(int id)
        {
            try
            {
                var response = new ClassCourseMappingResponse();
                string query = @"
        SELECT 
            cc.CourseClassMappingID,
            cc.CourseID,
            cc.ClassID,
            cc.Status,
            cc.createdon,
            cc.EmployeeID,
            cc.modifiedon,
            cc.modifiedby,
            cl.classname,
            c.coursename
        FROM 
            tblClassCourses cc
        INNER JOIN tblClasses cl ON cc.ClassID = cl.ClassID
        INNER JOIN tblCourses c ON cc.CourseID = c.CourseID
        WHERE 
            cc.CourseClassMappingID = @CourseClassMappingID";

                var data = await _connection.QueryAsync<dynamic>(query, new { CourseClassMappingID = id });

                if (data == null || !data.Any())
                {
                    return new ServiceResponse<ClassCourseMappingResponse>(false, "Record not Found", new ClassCourseMappingResponse(), 500);
                }

                var firstRecord = data.First();
                response.CourseClassMappingID = firstRecord.CourseClassMappingID;
                response.ClassID = firstRecord.ClassID;
                response.Status = firstRecord.Status;
                response.createdon = firstRecord.createdon;
                response.EmployeeID = firstRecord.EmployeeID;
                response.classname = firstRecord.classname;
                response.modifiedby = firstRecord.modifiedby;
                response.modifiedon = firstRecord.modifiedon;

                response.Courses = data.Select(item => new CourseData
                {
                    CourseID = item.CourseID,
                    Coursename = item.coursename
                }).ToList();

                return new ServiceResponse<ClassCourseMappingResponse>(true, "Record Found", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ClassCourseMappingResponse>(false, ex.Message, new ClassCourseMappingResponse(), 500);
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
        public async Task<ServiceResponse<string>> AddUpdateClassCourseMapping(ClassCourseMappingDTO request)
        {
            try
            {
                var classData = await _connection.QueryAsync<Class>("SELECT * FROM tblClass WHERE ClassId = @ClassId", new { ClassId = request.ClassID });
                if (classData != null)
                {
                    // Check for existing course mappings
                    var existingMappings = await _connection.QueryAsync<int>(
                        "SELECT CourseID FROM tblClassCourses WHERE ClassID = @ClassID",
                        new { request.ClassID });

                    var existingCourseIDs = existingMappings.ToList();

                    if (request.CourseClassMappingID == 0)
                    {
                        foreach (var courseId in request.CourseID ??= [])
                        {
                            if (existingCourseIDs.Contains(courseId))
                            {
                                return new ServiceResponse<string>(false, $"Course ID {courseId} already exists for Class ID {request.ClassID}", string.Empty, StatusCodes.Status400BadRequest);
                            }

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
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                            }
                        }
                        return new ServiceResponse<string>(true, "Operation Successful", "Record added successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        // Delete existing mappings for the class
                        string delete = "DELETE FROM tblClassCourses WHERE ClassID = @ClassID";
                        int deletedRows = await _connection.ExecuteAsync(delete, new { request.ClassID });

                        foreach (var courseId in request.CourseID ??= [])
                        {
                            if (existingCourseIDs.Contains(courseId))
                            {
                                return new ServiceResponse<string>(false, $"Course ID {courseId} already exists for Class ID {request.ClassID}", string.Empty, StatusCodes.Status400BadRequest);
                            }

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
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                            }
                        }
                        return new ServiceResponse<string>(true, "Operation Successful", "Record updated successfully", StatusCodes.Status200OK);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation Failed", "No record found for class or course", 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}