﻿using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace Config_API.Repository.Implementations
{
    public class QuestionTypeRepository : IQuestionTypeRepository
    {
        private readonly IDbConnection _connection;

        public QuestionTypeRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionType(Questiontype request)
        {
            try
            {
                if (request.QuestionTypeID == 0)
                {
                    string query = @"
        INSERT INTO [tblQBQuestionType] (QuestionType, Code, Status, MinNoOfOptions, createdon, createdby, EmployeeID, TypeOfOption, Question)
        VALUES (@QuestionType, @Code, @Status, @MinNoOfOptions, @createdon, @createdby, @EmployeeID, @TypeOfOption, @Question);";
                    int insertedValue = await _connection.ExecuteAsync(query, new
                    {
                        request.MinNoOfOptions,
                        request.TypeOfOption,
                        request.Code,
                        request.QuestionType,
                        Status = true,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.Question
                    });
                    if (insertedValue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Type Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    // Update existing board

                    string query = @"
        UPDATE [tblQBQuestionType]
        SET QuestionType = @QuestionType,
            Code = @Code,
            Status = @Status,
            MinNoOfOptions = @MinNoOfOptions,
            modifiedon = @modifiedon,
            modifiedby = @modifiedby,
            EmployeeID = @EmployeeID,
            TypeOfOption = @TypeOfOption,
            Question = @Question
        WHERE QuestionTypeID = @QuestionTypeID;";
                    int rowsAffected = await _connection.ExecuteAsync(query, new
                    {
                        request.MinNoOfOptions,
                        request.TypeOfOption,
                        request.Code,
                        request.QuestionType,
                        request.Status,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.EmployeeID,
                        request.QuestionTypeID,
                        request.Question
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Type Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status404NotFound);
                    }

                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Question Type already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<QuestionTypeResponse>> GetQuestionTypeByID(int Id)
        {
            try
            {
                string query = @"
        SELECT 
            qt.QuestionTypeID,
            qt.QuestionType,
            qt.Code,
            qt.Status,
            qt.MinNoOfOptions,
            qt.modifiedon,
            qt.modifiedby,
            qt.createdon,
            qt.createdby,
            qt.EmployeeID,
            qt.TypeOfOption AS TypeOfOptionId,
            ot.OptionTypeName AS TypeOfOptionName,
            qt.Question
        FROM 
            [tblQBQuestionType] qt
        LEFT JOIN 
            [tblQBOptionType] ot ON qt.TypeOfOption = ot.OptionTypeId
        WHERE 
            qt.QuestionTypeID = @QuestionTypeID;";

                var data = await _connection.QueryFirstOrDefaultAsync<QuestionTypeResponse>(query, new { QuestionTypeID = Id });

                if (data != null)
                {
                    return new ServiceResponse<QuestionTypeResponse>(true, "Records Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<QuestionTypeResponse>(false, "Records Not Found", new QuestionTypeResponse(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionTypeResponse>(false, ex.Message, new QuestionTypeResponse(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypeList(GetAllQuestionTypeRequest request)
        {
            try
            {
                string query = @"
        SELECT 
            qt.QuestionTypeID,
            qt.QuestionType,
            qt.Code,
            qt.Status,
            qt.MinNoOfOptions,
            qt.Question,
            qt.modifiedon,
            qt.modifiedby,
            qt.createdon,
            qt.createdby,
            qt.EmployeeID,
            qt.TypeOfOption AS TypeOfOptionId,
            ot.OptionTypeName AS TypeOfOptionName
        FROM 
            [tblQBQuestionType] qt
        LEFT JOIN 
            [tblQBOptionType] ot ON qt.TypeOfOption = ot.OptionTypeId;";

                var data = await _connection.QueryAsync<QuestionTypeResponse>(query);
                var paginatedList = data.Skip((request.PageNumber - 1) * request.PageSize)
             .Take(request.PageSize)
             .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, data.Count());
                }
                else
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeResponse>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypeListMasters()
        {
            try
            {
                string query = @"
        SELECT 
            qt.QuestionTypeID,
            qt.QuestionType,
            qt.Code,
            qt.Status,
            qt.MinNoOfOptions,
            qt.Question,
            qt.modifiedon,
            qt.modifiedby,
            qt.createdon,
            qt.createdby,
            qt.EmployeeID,
            qt.TypeOfOption AS TypeOfOptionId,
            ot.OptionTypeName AS TypeOfOptionName
        FROM 
            [tblQBQuestionType] qt
        LEFT JOIN 
            [tblQBOptionType] ot ON qt.TypeOfOption = ot.OptionTypeId where Status = 1;";

                var data = await _connection.QueryAsync<QuestionTypeResponse>(query);

                if (data.Any())
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeResponse>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList()
        {

            try
            {
                string query = @"SELECT * FROM [tblQBNoOfOptions];";

                var data = await _connection.QueryAsync<NoOfOptions>(query);

                if (data.Any())
                {
                    return new ServiceResponse<List<NoOfOptions>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<NoOfOptions>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<NoOfOptions>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<OptionType>>> OptionTypesList()
        {
            try
            {
                string query = @"SELECT * FROM [tblQBOptionType];";

                var data = await _connection.QueryAsync<OptionType>(query);

                if (data.Any())
                {
                    return new ServiceResponse<List<OptionType>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<OptionType>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<OptionType>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetQuestionTypeByID(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status;

                    string sql = "UPDATE [tblQBQuestionType] SET Status = @Status WHERE QuestionTypeID = @QuestionTypeID";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { data.Data.Status, QuestionTypeID = id });
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
