using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using System.Data;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Config_API.Repository.Implementations
{
    public class TypeOfTestSeriesRepository : ITypeOfTestSeriesRepository
    {
        private readonly IDbConnection _connection;

        public TypeOfTestSeriesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateTestSeries(TypeOfTestSeries request)
        {
            try
            {
                if (request.TTSId == 0)
                {
                    var query = @"
                INSERT INTO [tblTypeOfTestSeries] 
                (TestSeriesName, TestSeriesCode, Status, createdon, createdby, EmployeeID, EmpFirstName)
                VALUES 
                (@TestSeriesName, @TestSeriesCode, @Status, @createdon, @createdby, @EmployeeID, @EmpFirstName)";

                    int insertedValue = await _connection.ExecuteAsync(query, new
                    {
                        request.TestSeriesName,
                        request.TestSeriesCode,
                        Status = true,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.EmpFirstName
                    });
                    if (insertedValue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Type Of Test Series Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {

                    var query = @"
                UPDATE [tblTypeOfTestSeries]
                SET 
                    TestSeriesName = @TestSeriesName,
                    TestSeriesCode = @TestSeriesCode,
                    Status = @Status,
                    modifiedon = @modifiedon,
                    modifiedby = @modifiedby,
                    EmployeeID = @EmployeeID,
                    EmpFirstName = @EmpFirstName
                WHERE 
                    TTSId = @TTSId";

                    int rowsAffected = await _connection.ExecuteAsync(query, new
                    {
                        request.TTSId,
                        request.TestSeriesName,
                        request.TestSeriesCode,
                        request.Status,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.EmployeeID,
                        request.EmpFirstName
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Type Of Test Series Updated Successfully", StatusCodes.Status200OK);
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

        public async Task<ServiceResponse<List<TypeOfTestSeries>>> GetListOfTestSeries()
        {
            try
            {
                string sql = @"SELECT *
                            FROM tblTypeOfTestSeries";

                var data = await _connection.QueryAsync<TypeOfTestSeries>(sql);

                if (data != null)
                {
                    return new ServiceResponse<List<TypeOfTestSeries>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<TypeOfTestSeries>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TypeOfTestSeries>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<TypeOfTestSeries>> GetTestSeriesById(int id)
        {
            try
            {
                var query = @"
                SELECT 
                    TTSId,
                    TestSeriesName,
                    TestSeriesCode,
                    Status,
                    createdon,
                    createdby,
                    modifiedon,
                    modifiedby,
                    EmployeeID,
                    EmpFirstName
                FROM 
                    [tblTypeOfTestSeries]
                WHERE 
                    TTSId = @TTSId";

                var data = await _connection.QuerySingleOrDefaultAsync<TypeOfTestSeries>(query, new { TTSId = id });

                if (data != null)
                {
                    return new ServiceResponse<TypeOfTestSeries>(true, "Records Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<TypeOfTestSeries>(false, "Records Not Found", new TypeOfTestSeries(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TypeOfTestSeries>(false, ex.Message, new TypeOfTestSeries(), StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetTestSeriesById(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status;

                    string sql = "UPDATE tblTypeOfTestSeries SET Status = @Status WHERE TTSId = @TTSId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { data.Data.Status, TTSId = id });
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
