using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

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
        INSERT INTO [tblQBQuestionType] (QuestionType, Code, Status, MinNoOfOptions, createdon, createdby, EmployeeID, TypeOfOption, EmpFirstName)
        VALUES (@QuestionType, @Code, @Status, @MinNoOfOptions, @createdon, @createdby, @EmployeeID, @TypeOfOption. @EmpFirstName);";
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
                        request.EmpFirstName
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
            EmpFirstName = @EmpFirstName
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
                        request.EmployeeID
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
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<Questiontype>> GetQuestionTypeByID(int Id)
        {
            try
            {
                string query = @"
        SELECT 
            QuestionTypeID,
            QuestionType,
            Code,
            Status,
            MinNoOfOptions,
            modifiedon,
            modifiedby,
            createdon,
            createdby,
            EmployeeID,
            TypeOfOption,
            EmpFirstName
        FROM 
            [tblQBQuestionType]
        WHERE 
            QuestionTypeID = @QuestionTypeID;";

                var data = await _connection.QueryFirstOrDefaultAsync<Questiontype>(query, new { QuestionTypeID = Id });

                if (data != null)
                {
                    return new ServiceResponse<Questiontype>(true, "Records Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Questiontype>(false, "Records Not Found", new Questiontype(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Questiontype>(false, ex.Message, new Questiontype(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Questiontype>>> GetQuestionTypeList()
        {
            try
            {
                string query = @"SELECT [QuestionTypeID],[QuestionType],[Code],[Status],[MinNoOfOptions]
                                    ,[modifiedon],[modifiedby],[createdon],[createdby],[EmployeeID],[TypeOfOption], EmpFirstName
                                    FROM [tblQBQuestionType];";

                var data = await _connection.QueryAsync<Questiontype>(query);

                if (data != null)
                {
                    return new ServiceResponse<List<Questiontype>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Questiontype>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Questiontype>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<NoOfOptions>>> NoOfOptionsList()
        {

            try
            {
                string query = @"SELECT * FROM [tblQBNoOfOptions];";

                var data = await _connection.QueryAsync<NoOfOptions>(query);

                if (data != null)
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

                if (data != null)
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
