using Config_API.DTOs.Requests;
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
                    string insertSql = @"INSERT INTO tblSubject ([SubjectName], [SubjectCode], [Status], [createdby], [createdon], [EmployeeID])
                           VALUES (@SubjectName, @SubjectCode, @Status, @CreatedBy, GETDATE(), @EmployeeID)";
                    
                    int rowsAffected = await _connection.ExecuteAsync(insertSql, new
                    {
                        request.SubjectName,
                        request.SubjectCode,
                        Status = true,
                        request.createdby,
                        createdon = DateTime.Now,
                        request.EmployeeID,
                    });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Subject Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    string updateSql = @"UPDATE [tblSubject]
                           SET [SubjectName] = @SubjectName, [SubjectCode] = @SubjectCode, [Status] = @Status, [modifiedby] = @ModifiedBy, [modifiedon] = GETDATE(), EmployeeID = @EmployeeID
                           WHERE [SubjectId] = @SubjectId";

                    int rowsAffected = await _connection.ExecuteAsync(updateSql, new
                    {
                        request.SubjectId,
                        request.SubjectName,
                        request.SubjectCode,
                        request.Status,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.EmployeeID
                    });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Subject Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "Subject does not exist", StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Subject>>> GetAllSubjects(GetAllSubjectsRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblSubject]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);

                // Construct the SQL query to select all subjects
                string query = "SELECT [SubjectId], [SubjectName], [SubjectCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [groupname], [icon], [colorcode], [subjecttype], [EmployeeID], EmpFirstName FROM [tblSubject]";

                // Execute the select query asynchronously
                var data = await _connection.QueryAsync<Subject>(query);
                var paginatedList = data.Skip((request.PageNumber - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Subject>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<Subject>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Subject>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Subject>>> GetAllSubjectsMAsters()
        {
            try
            {
                // Construct the SQL query to select all subjects
                string query = "SELECT [SubjectId], [SubjectName], [SubjectCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [groupname], [icon], [colorcode], [subjecttype], [EmployeeID], EmpFirstName FROM [tblSubject]";

                // Execute the select query asynchronously
                var data = await _connection.QueryAsync<Subject>(query);

                if (data.Any())
                {
                    return new ServiceResponse<List<Subject>>(true, "Records Found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Subject>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Subject>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<Subject>> GetSubjectById(int id)
        {

            try
            {
                string query = "SELECT [SubjectId], [SubjectName], [SubjectCode], [Status], [createdby], [createdon], [displayorder], [modifiedby], [modifiedon], [groupname], [icon], [colorcode], [subjecttype], [EmployeeID], EmpFirstName FROM [tblSubject] WHERE [SubjectId] = @SubjectId";

                var data = await _connection.QueryFirstOrDefaultAsync<Subject>(query, new { SubjectId = id });

                if (data != null)
                {
                    return new ServiceResponse<Subject>(true, "Record Found", data, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Subject>(false, "Record not Found", new Subject(), StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Subject>(false, ex.Message, new Subject(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetSubjectById(id);
                if (data.Data != null)
                {
                    // Toggle the status
                    data.Data.Status = !data.Data.Status;

                    string sql1 = "UPDATE tblSubject SET Status = @Status WHERE SubjectId = @SubjectId";

                    int rowsAffected = await _connection.ExecuteAsync(sql1, new { data.Data.Status, SubjectId = id });

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
