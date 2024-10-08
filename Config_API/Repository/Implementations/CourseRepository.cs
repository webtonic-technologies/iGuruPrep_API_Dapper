﻿using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;
using System.Data.SqlClient;

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
                        EmployeeID = request.EmployeeID,
                        Status = true
                    };

                    string insertQuery = @"INSERT INTO [tblCourse] 
                           ([CourseName], [CourseCode], [Status], [createdby], [createdon], [EmployeeID])
                           VALUES (@CourseName, @CourseCode, @Status, @CreatedBy, GETDATE(), @EmployeeID)";
                    
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
                           [modifiedby] = @ModifiedBy, 
                           [modifiedon] = GETDATE(), 
                           [EmployeeID] = @EmployeeID
                           WHERE [CourseId] = @CourseId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.CourseName,
                        request.CourseCode,
                        request.Status,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.EmployeeID,
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
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Course name or code already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }

        }
        public async Task<ServiceResponse<List<Course>>> GetAllCourses(GetAllCoursesRequest request)
        {
            try
            {
                string query = @"SELECT [CourseId], [CourseName], [CourseCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [EmployeeID], [EmpFirstName]
                           FROM [tblCourse]";
                var data = await _connection.QueryAsync<Course>(query);
                var paginatedList = data.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Course>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, data.Count());
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
        //public async Task<ServiceResponse<List<Course>>> GetAllCoursesMasters()
        //{
        //    try
        //    {
        //        string query = @"SELECT [CourseId], [CourseName], [CourseCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [EmployeeID], [EmpFirstName]
        //                   FROM [tblCourse] where Status = 1";
        //        var data = await _connection.QueryAsync<Course>(query);

        //        if (data.Any())
        //        {
        //            return new ServiceResponse<List<Course>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<Course>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<Course>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
        //    }
        //}
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
        public async Task<ServiceResponse<List<Course>>> GetAllCoursesMasters(int classId)
        {
            try
            {
                // SQL query to fetch courses based on the provided class ID
                string query = @"
            SELECT c.CourseId, c.CourseName, c.CourseCode, c.Status, 
                   c.createdby, c.createdon, c.displayorder, 
                   c.modifiedby, c.modifiedon, 
                   cc.EmployeeID, cc.EmpFirstName
            FROM tblCourse c
            INNER JOIN tblClassCourses cc ON c.CourseId = cc.CourseID
            WHERE cc.ClassID = @ClassId AND c.Status = 1";

                var data = await _connection.QueryAsync<Course>(query, new { ClassId = classId });

                if (data.Any())
                {
                    return new ServiceResponse<List<Course>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Course>>(false, "Records Not Found", new List<Course>(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Course>>(false, ex.Message, new List<Course>(), StatusCodes.Status500InternalServerError);
            }
        }

    }
}
