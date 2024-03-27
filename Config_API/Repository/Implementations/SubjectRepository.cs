using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class SubjectRepository : ISubjectRepository
    {

        private readonly IDbConnection _connection;

        public SubjectRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateSubject(Subject request)
        {
            try
            {
                if (request.SubjectId == 0)
                {
                    string insertSql = @"INSERT INTO tblSubject (ColorCode, SubjectName, SubjectCode, CreatedBy, CreatedOn, DisplayOrder, GroupName, Icon, ModifiedBy, ModifiedOn, Status, SubjectType)
                                 VALUES (@ColorCode, @SubjectName, @SubjectCode, @CreatedBy, @CreatedOn, @DisplayOrder, @GroupName, @Icon, @ModifiedBy, @ModifiedOn, @Status, @SubjectType)";

                    int rowsAffected = await _connection.ExecuteAsync(insertSql, request);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Subject Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    string updateSql = @"UPDATE tblSubject
                                 SET ColorCode = @ColorCode, SubjectName = @SubjectName, SubjectCode = @SubjectCode, CreatedBy = @CreatedBy, CreatedOn = @CreatedOn, 
                                     DisplayOrder = @DisplayOrder, GroupName = @GroupName, Icon = @Icon, ModifiedBy = @ModifiedBy, ModifiedOn = @ModifiedOn,
                                     Status = @Status, SubjectType = @SubjectType
                                 WHERE SubjectId = @SubjectId";

                    int rowsAffected = await _connection.ExecuteAsync(updateSql, request);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Subject Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "Subject does not exist", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Subject>>> GetAllSubjects()
        {
            try
            {
                // Construct the SQL query to select all subjects
                string sql = "SELECT * FROM tblSubject";

                // Execute the select query asynchronously
                var data = await _connection.QueryAsync<Subject>(sql);

                if (data != null)
                {
                    return new ServiceResponse<List<Subject>>(true, "Records Found", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Subject>>(false, "Records Not Found", new List<Subject>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Subject>>(false, ex.Message, new List<Subject>(), 200);
            }
        }

        public async Task<ServiceResponse<Subject>> GetSubjectById(int id)
        {

            try
            {
                string sql = "SELECT * FROM tblSubject WHERE SubjectId = @SubjectId";

                var data = await _connection.QueryFirstOrDefaultAsync<Subject>(sql, new { SubjectId = id });

                if (data != null)
                {
                    return new ServiceResponse<Subject>(true, "Record Found", data, 200);
                }
                else
                {
                    return new ServiceResponse<Subject>(false, "Record not Found", new Subject(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Subject>(false, ex.Message, new Subject(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                string sql = "SELECT * FROM tblSubject WHERE SubjectId = @SubjectId";
               var data =  await _connection.QueryFirstOrDefaultAsync<Subject>(sql, new { SubjectId = id });


                if (data != null)
                {
                    // Toggle the status
                    data.Status = !data.Status;

                    string sql1 = "UPDATE tblSubject SET Status = @Status WHERE SubjectId = @SubjectId";

                    int rowsAffected = await _connection.ExecuteAsync(sql1, new { data.Status, SubjectId = id });

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
