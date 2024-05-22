using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using System.Data;
using Dapper;

namespace Config_API.Repository.Implementations
{
    public class ContentIndexRepository : IContentIndexRepository
    {

        private readonly IDbConnection _connection;

        public ContentIndexRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndex request)
        {
            try
            {
                if (request.SubjectIndexId == 0)
                {
                    // Insert new board
                    string insertQuery = @"
                    INSERT INTO tblQBSubjectContentIndex (SubjectId, ContentName, IndexTypeId, ParentLevel, Status,
                    ClassId, BoardId, APID, CreatedOn, CreatedBy, CourseId, PathURL, APName, SubjectName, EmployeeID, EmpFirstName)
                    VALUES ( @SubjectId, @ContentName, @IndexTypeId, @ParentLevel, @Status, @ClassId, @BoardId, @APID, @CreatedOn, 
                    @CreatedBy, @CourseId, @PathURL, @APName, @SubjectName, @EmployeeID, @EmpFirstName);";
                    int insertedValue = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.SubjectId,
                        request.ContentName,
                        request.IndexTypeId,
                        request.ParentLevel,
                        Status = true,
                        request.classid,
                        request.boardid,
                        request.APID,
                        CreatedOn = DateTime.Now,
                        request.CreatedBy,
                        request.courseid,
                        request.pathURL,
                        request.APName,
                        request.Subjectname,
                        request.EmployeeID,
                        request.EmpFirstName
                    });
                    if (insertedValue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    // Update existing board
                    string updateQuery = @"
            UPDATE tblQBSubjectContentIndex SET
                SubjectId = @SubjectId,
                ContentName = @ContentName,
                IndexTypeId = @IndexTypeId,
                ParentLevel = @ParentLevel,
                Status = @Status,
                ClassId = @ClassId,
                BoardId = @BoardId,
                APID = @APID,
                ModifiedOn = @ModifiedOn,
                ModifiedBy = @ModifiedBy,
                CourseId = @CourseId,
                PathURL = @PathURL,
                APName = @APName,
                SubjectName = @SubjectName,
                EmployeeID = @EmployeeID,
                EmpFirstName = @EmpFirstName
            WHERE SubjectIndexId = @SubjectIndexId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.SubjectIndexId,
                        request.SubjectId,
                        request.ContentName,
                        request.IndexTypeId,
                        request.ParentLevel,
                        request.Status,
                        request.classid,
                        request.boardid,
                        request.APID,
                        ModifiedOn = DateTime.Now,
                        request.ModifiedBy,
                        request.courseid,
                        request.pathURL,
                        request.APName,
                        request.Subjectname,
                        request.EmployeeID,
                        request.EmpFirstName
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Updated Successfully", StatusCodes.Status200OK);
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

        public async Task<ServiceResponse<List<ContentIndex>>> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {
                string query = @"SELECT * FROM [tblQBSubjectContentIndex] WHERE 1 = 1";

                // Add filters based on DTO properties
                if (request.APID > 0)
                {
                    query += " AND [APID] = @APID";
                }
                if (request.SubjectId > 0)
                {
                    query += " AND [SubjectId] = @SubjectId";
                }

                var data = await _connection.QueryAsync<ContentIndex>(query, request);
                if (data != null)
                {
                    return new ServiceResponse<List<ContentIndex>>(true, "Records found", data.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<ContentIndex>>(false, "Records not found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndex>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<ContentIndex>> GetContentIndexById(int id)
        {
            try
            {
                string sql = @"SELECT * FROM [tblQBSubjectContentIndex] WHERE [SubjectIndexId] = @id";

                var Content = await _connection.QueryFirstOrDefaultAsync<ContentIndex>(sql, new { id });

                if (Content != null)
                {
                    return new ServiceResponse<ContentIndex>(true, "Records Found", Content, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<ContentIndex>(false, "Records Not Found", new ContentIndex(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentIndex>(false, ex.Message, new ContentIndex(), StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var data = await GetContentIndexById(id);

                if (data.Data != null)
                {
                    data.Data.Status = !data.Data.Status; // Toggle the status

                    string updateQuery = @"UPDATE [tblQBSubjectContentIndex] SET Status = @Status WHERE [SubjectIndexId] = @Id";
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
    }
}
