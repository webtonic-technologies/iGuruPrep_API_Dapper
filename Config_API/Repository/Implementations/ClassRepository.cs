﻿using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;
using System.Data.SqlClient;

namespace Config_API.Repository.Implementations
{
    public class ClassRepository : IClassRepository
    {

        private readonly IDbConnection _connection;

        public ClassRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateClass(Class request)
        {
            try
            {
                if (request.ClassId == 0)
                {
                    var newClass = new Class
                    {
                        ClassCode = request.ClassCode,
                        ClassName = request.ClassName,
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        EmployeeID = request.EmployeeID,
                        Status = true
                    };
                    string insertQuery = @"INSERT INTO [tblClass] 
                           ([ClassName], [ClassCode], [Status], [createdby], [createdon], [EmployeeID])
                           VALUES (@ClassName, @ClassCode, @Status, @CreatedBy, @CreatedOn, @EmployeeID)";
                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newClass);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE [tblClass] 
                           SET [ClassName] = @ClassName, [ClassCode] = @ClassCode, [Status] = @Status, 
                               [modifiedby] = @ModifiedBy, [modifiedon] = @ModifiedOn, [EmployeeID] = @EmployeeID
                           WHERE [ClassId] = @ClassId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.ClassName,
                        request.ClassCode,
                        request.Status,
                        request.EmployeeID,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.ClassId
                    });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Class Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Class name or code already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Class>>> GetAllClasses(GetAllClassesRequest request)
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
                           FROM [tblClass]";
                var classes = await _connection.QueryAsync<Class>(query);
                var paginatedList = classes.Skip((request.PageNumber - 1) * request.PageSize)
                         .Take(request.PageSize)
                         .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Class>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, classes.Count());
                }
                else
                {
                    return new ServiceResponse<List<Class>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Class>>> GetAllClassesMaster()
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
                var classes = await _connection.QueryAsync<Class>(query);

                if (classes.Any())
                {
                    return new ServiceResponse<List<Class>>(true, "Records Found", classes.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Class>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Class>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<Class>> GetClassById(int id)
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
                           FROM [tblClass]
                           WHERE [ClassId] = @ClassId";

                var classObj = await _connection.QueryFirstOrDefaultAsync<Class>(query, new { ClassId = id });

                if (classObj != null)
                {
                    return new ServiceResponse<Class>(true, "Record Found", classObj, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Class>(false, "Record not Found", new Class(), StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Class>(false, ex.Message, new Class(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                string query = @"
                SELECT ClassId, Status
                FROM tblClass
                WHERE ClassId = @Id";

                var classObj = await _connection.QueryFirstOrDefaultAsync<Class>(query, new { Id = id });

                if (classObj != null)
                {
                    // Toggle the status
                    classObj.Status = !classObj.Status;

                    string updateQuery = @"
                    UPDATE tblClass
                    SET Status = @Status
                    WHERE ClassId = @Id";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { classObj.Status, Id = id });

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
