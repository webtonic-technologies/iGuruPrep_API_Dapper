using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class QuestionLevelRepository : IQuestionLevelRepository
    {

        private readonly IDbConnection _connection;

        public QuestionLevelRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestionLevel(QuestionLevel request)
        {
            try
            {
                if (request.LevelId == 0)
                {
                    var newQuestionLevel = new QuestionLevel
                    {
                        CreatedOn = DateTime.Now,
                        LevelCode = request.LevelCode,
                        LevelName = request.LevelName,
                        PatternCode = request.PatternCode,
                        Status = request.Status
                    };

                    string insertQuery = @"INSERT INTO tblDifficultyLevel (CreatedOn, LevelCode, LevelName, PatternCode, Status)
                               VALUES (@CreatedOn, @LevelCode, @LevelName, @PatternCode, @Status)";

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, newQuestionLevel);


                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Level Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"UPDATE tblDifficultyLevel
                               SET LevelName = @LevelName, Status = @Status, PatternCode = @PatternCode, LevelCode = @LevelCode
                               WHERE LevelId = @LevelId";

                    var parameters = new
                    {
                        request.LevelName,
                        request.Status,
                        request.PatternCode,
                        request.LevelCode,
                        request.LevelId
                    };

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Level Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<QuestionLevel>>> GetAllQuestionLevel()
        {
            try
            {
                string query = @"SELECT * FROM tblDifficultyLevel";

                var data = await _connection.QueryAsync<QuestionLevel>(query);

                if (data != null)
                {
                    return new ServiceResponse<List<QuestionLevel>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<QuestionLevel>>(false, "Records Not Found", new List<QuestionLevel>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionLevel>>(false, ex.Message, new List<QuestionLevel>(), 200);
            }

        }

        public async Task<ServiceResponse<QuestionLevel>> GetQuestionLevelById(int id)
        {
            try
            {
                string query = @"SELECT * FROM tblDifficultyLevel
                     WHERE LevelId = @LevelId";

                var data = await _connection.QueryFirstOrDefaultAsync<QuestionLevel>(query, new { LevelId = id });
                if (data != null)
                {
                    return new ServiceResponse<QuestionLevel>(true, "Record Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<QuestionLevel>(false, "Record not Found", new QuestionLevel(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionLevel>(false, ex.Message, new QuestionLevel(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                string sql = "SELECT * FROM tblDifficultyLevel WHERE LevelId = @LevelId";
                var data = await _connection.QueryFirstOrDefaultAsync<QuestionLevel>(sql, new { LevelId = id });

                if (data != null)
                {
                    // Toggle the status
                    data.Status = !data.Status;

                    string sql1 = "UPDATE tblDifficultyLevel SET Status = @Status WHERE LevelId = @LevelId";

                    int rowsAffected = await _connection.ExecuteAsync(sql1, new { data.Status, LevelId = id });
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
