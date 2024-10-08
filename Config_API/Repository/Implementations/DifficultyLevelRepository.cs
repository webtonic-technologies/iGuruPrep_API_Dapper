﻿using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace Config_API.Repository.Implementations
{
    public class DifficultyLevelRepository : IDifficultyLevelRepository
    {

        private readonly IDbConnection _connection;

        public DifficultyLevelRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionLevel(DifficultyLevel request)
        {
            try
            {
                if (request.LevelId == 0)
                {
                    var newQuestionLevel = new DifficultyLevel
                    {
                        createdby = request.createdby,
                        createdon = DateTime.Now,
                        EmployeeID = request.EmployeeID,
                        LevelCode = request.LevelCode,
                        LevelName = request.LevelName,
                        NoofQperLevel = request.NoofQperLevel,
                        Status = true,
                        SuccessRate = request.SuccessRate
                    };

                    string insertQuery = @"INSERT INTO [tbldifficultylevel] 
                             ([LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], [createdon], [createdby], [EmployeeID])
                             VALUES (@LevelName, @LevelCode, @Status, @NoofQperLevel, @SuccessRate, @createdon, @createdby, @EmployeeID)";

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newQuestionLevel);


                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Level Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE [tbldifficultylevel] SET 
                             [LevelName] = @LevelName, [LevelCode] = @LevelCode, [Status] = @Status, [NoofQperLevel] = @NoofQperLevel, 
                             [SuccessRate] = @SuccessRate, [modifiedon] = @modifiedon, 
                             [modifiedby] = @modifiedby, [EmployeeID] = @EmployeeID
                             WHERE [LevelId] = @LevelId";

                    var parameters = new
                    {
                        request.LevelId,
                        request.EmployeeID,
                        request.LevelCode,
                        request.LevelName,
                        request.NoofQperLevel,
                        request.Status,
                        request.SuccessRate,
                        modifiedon = DateTime.Now,
                        request.modifiedby
                    };

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Level Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Difficulty level already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevel(GetAllDifficultyLevelRequest request)
        {
            try
            {
                string query = @"SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], 
                             [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], EmpFirstName
                             FROM [tbldifficultylevel]";

                var data = await _connection.QueryAsync<DifficultyLevel>(query);
                var paginatedList = data.Skip((request.PageNumber - 1) * request.PageSize)
                  .Take(request.PageSize)
                  .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<DifficultyLevel>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, data.Count());
                }
                else
                {
                    return new ServiceResponse<List<DifficultyLevel>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<DifficultyLevel>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }

        }
        public async Task<ServiceResponse<List<DifficultyLevel>>> GetAllQuestionLevelMasters()
        {
            try
            {
                string query = @"SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], 
                             [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], EmpFirstName
                             FROM [tbldifficultylevel] where Status = 1";

                var data = await _connection.QueryAsync<DifficultyLevel>(query);
             
                if (data.Any())
                {
                    return new ServiceResponse<List<DifficultyLevel>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<DifficultyLevel>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<DifficultyLevel>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<ServiceResponse<DifficultyLevel>> GetQuestionLevelById(int id)
        {
            try
            {
                string query = @"SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], 
                             [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], EmpFirstName
                             FROM [tbldifficultylevel]
                             WHERE [LevelId] = @LevelId";

                var data = await _connection.QueryFirstOrDefaultAsync<DifficultyLevel>(query, new { LevelId = id });
                if (data != null)
                {
                    return new ServiceResponse<DifficultyLevel>(true, "Record Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<DifficultyLevel>(false, "Record not Found", new DifficultyLevel(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<DifficultyLevel>(false, ex.Message, new DifficultyLevel(), StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {

                var data = await GetQuestionLevelById(id);

                if (data.Data != null)
                {
                    // Toggle the status
                    data.Data.Status = !data.Data.Status;

                    string sql1 = "UPDATE tblDifficultyLevel SET Status = @Status WHERE LevelId = @LevelId";

                    int rowsAffected = await _connection.ExecuteAsync(sql1, new { data.Data.Status, LevelId = id });
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
