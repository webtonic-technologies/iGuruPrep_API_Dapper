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
                string query = @"
        SELECT 
            cc.CourseClassMappingID,
            cc.CourseID,
            cc.ClassID,
            cc.Status,
            cc.createdon,
            cc.EmployeeID,
            e.EmpFirstName AS EmpFirstName,
            cc.modifiedon,
            cc.modifiedby,
            cl.classname,
            c.coursename
        FROM 
            tblClassCourses cc
        INNER JOIN tblClass cl ON cc.ClassID = cl.ClassID
        INNER JOIN tblCourse c ON cc.CourseID = c.CourseID
        LEFT JOIN tblEmployee e ON cc.EmployeeID = e.Employeeid";

                var classCourseMappings = await _connection.QueryAsync<dynamic>(query);

                var groupedMappings = classCourseMappings
                    .GroupBy(m => m.ClassID)
                    .Select(g => new ClassCourseMappingResponse
                    {
                        ClassID = g.Key,
                        Status = g.First().Status,
                        createdon = g.First().createdon,
                        EmployeeID = g.First().EmployeeID,
                        EmpFirstName = g.First().EmpFirstName,
                        modifiedon = g.First().modifiedon,
                        modifiedby = g.First().modifiedby,
                        classname = g.First().classname,
                        Courses = g.Select(m => new CourseData
                        {
                            CourseClassMappingID = m.CourseClassMappingID,
                            CourseID = m.CourseID,
                            Coursename = m.coursename,
                          
                        }).ToList()
                    }).ToList();

                var paginatedList = groupedMappings
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<ClassCourseMappingResponse>>(true, "Records Found", paginatedList, 200, groupedMappings.Count);
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
        public async Task<ServiceResponse<ClassCourseMappingResponse>> GetClassCourseMappingById(int courseClassMappingID)
        {
            try
            {
                // Query to get ClassID from CourseClassMappingID
                string classIdQuery = @"
                SELECT 
                    ClassID 
                FROM 
                    tblClassCourses 
                WHERE 
                    CourseClassMappingID = @CourseClassMappingID";

                var classIdResult = await _connection.QuerySingleOrDefaultAsync<int>(classIdQuery, new { CourseClassMappingID = courseClassMappingID });

                if (classIdResult == 0)
                {
                    return new ServiceResponse<ClassCourseMappingResponse>(false, "Record not Found", new ClassCourseMappingResponse(), 500);
                }

                int classId = classIdResult;

                // Query to get all records associated with the ClassID
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
                    c.coursename,
                    e.EmpFirstName
                FROM 
                    tblClassCourses cc
                INNER JOIN tblClass cl ON cc.ClassID = cl.ClassID
                INNER JOIN tblCourse c ON cc.CourseID = c.CourseID
                LEFT JOIN tblEmployee e ON cc.EmployeeID = e.Employeeid
                WHERE 
                    cc.ClassID = @ClassID";

                var data = await _connection.QueryAsync<dynamic>(query, new { ClassID = classId });

                if (data == null || !data.Any())
                {
                    return new ServiceResponse<ClassCourseMappingResponse>(false, "Record not Found", new ClassCourseMappingResponse(), 500);
                }

                var firstRecord = data.First();
                var response = new ClassCourseMappingResponse
                {
                    ClassID = firstRecord.ClassID,
                    Status = firstRecord.Status,
                    createdon = firstRecord.createdon,
                    EmployeeID = firstRecord.EmployeeID,
                    classname = firstRecord.classname,
                    modifiedby = firstRecord.modifiedby,
                    modifiedon = firstRecord.modifiedon,
                    EmpFirstName = firstRecord.EmpFirstName,
                    Courses = data.Select(item => new CourseData
                    {
                        CourseClassMappingID = item.CourseClassMappingID,
                        CourseID = item.CourseID,
                        Coursename = item.coursename,
                    }).ToList()
                };

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
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                if (request.CourseID?.Count == 0)
                {
                    return new ServiceResponse<string>(false, "Courses data cannot be empty", string.Empty, StatusCodes.Status400BadRequest);
                }

                var classData = await _connection.QueryAsync<Class>("SELECT * FROM tblClass WHERE ClassId = @ClassId", new { ClassId = request.ClassID });
                if (classData != null)
                {
                    // Check for existing course mappings
                    var existingMappings = await _connection.QueryAsync<int>(
                        "SELECT CourseID FROM tblClassCourses WHERE ClassID = @ClassID",
                        new { request.ClassID });

                    var existingCourseIDs = existingMappings.ToList();

                    // Prepare the list of course IDs to be inserted and removed
                    var newCourseIDs = request.CourseID?.Except(existingCourseIDs).ToList();
                    var removedCourseIDs = existingCourseIDs?.Except(request.CourseID ??= ([])).ToList();

                    using (var transaction = _connection.BeginTransaction())
                    {
                        // Remove course mappings that are not in the new list
                        if (removedCourseIDs?.Count > 0)
                        {
                            string deleteQuery = "DELETE FROM tblClassCourses WHERE ClassID = @ClassID AND CourseID IN @CourseIDs";
                            int deletedRows = await _connection.ExecuteAsync(deleteQuery, new { request.ClassID, CourseIDs = removedCourseIDs }, transaction);

                            if (deletedRows == 0)
                            {
                                transaction.Rollback();
                                return new ServiceResponse<string>(false, "Failed to remove old course mappings", string.Empty, StatusCodes.Status400BadRequest);
                            }
                        }

                        // Insert new course mappings
                        if (newCourseIDs?.Count > 0)
                        {
                            foreach (var courseId in newCourseIDs)
                            {
                                string query = @"INSERT INTO tblClassCourses (CourseID, ClassID, Status, createdon, EmployeeID, modifiedon, modifiedby) 
                                         VALUES (@CourseID, @ClassID, @Status, @createdon, @EmployeeID, @modifiedon, @modifiedby)";

                                int rowsAffected = await _connection.ExecuteAsync(query, new
                                {
                                    CourseID = courseId,
                                    request.ClassID,
                                    Status = true,
                                    createdon = DateTime.Now,
                                    modifiedon = DateTime.UtcNow,
                                    request.modifiedby,
                                    request.EmployeeID
                                }, transaction);

                                if (rowsAffected == 0)
                                {
                                    transaction.Rollback();
                                    return new ServiceResponse<string>(false, "Failed to insert new course mappings", string.Empty, StatusCodes.Status400BadRequest);
                                }
                            }
                        }

                        transaction.Commit();
                        return new ServiceResponse<string>(true, "Operation Successful", "Courses updated successfully", StatusCodes.Status200OK);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation Failed", "No record found for class or course", StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}