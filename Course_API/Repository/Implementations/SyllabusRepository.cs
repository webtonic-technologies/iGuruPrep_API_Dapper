using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class SyllabusRepository : ISyllabusRepository
    {
        private readonly IDbConnection _connection;

        public SyllabusRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateSyllabus(Syllabus request)
        {
            try
            {
                if (request.SyllabusId == 0)
                {
                    string insertQuery = @"
        INSERT INTO tblSyllabus (BoardID, CourseId, ClassId, Description, SyllabusName,YearID, Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SubjectId)
        OUTPUT Inserted.SyllabusId
        VALUES (@BoardID, @CourseId, @ClassId, @Description, @SyllabusName,@YearID, @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn, @SubjectId)";

                    var syllabusId = await _connection.ExecuteScalarAsync<int>(insertQuery, request);
                    if (syllabusId != 0)
                    {
                        string insertSubSyQuery = @"
            INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
            VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

                        var syllabusSubject = new SyllabusSubject
                        {
                            CreatedBy = request.CreatedBy,
                            CreatedDate = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedDate = request.ModifiedOn,
                            Status = true,
                            SubjectID = request.SubjectId,
                            SyllabusID = syllabusId,
                        };
                        int rowsAffected = await _connection.ExecuteAsync(insertSubSyQuery, syllabusSubject);
                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
        UPDATE tblSyllabus
        SET BoardID = @BoardID,
            CourseId = @CourseId,
            ClassId = @ClassId,
            Description = @Description,
            YearID = @YearID,
            Status = @Status,
            SyllabusName = @SyllabusName,
            CreatedBy = @CreatedBy,
            CreatedOn = @CreatedOn,
            ModifiedBy = @ModifiedBy,
            ModifiedOn = @ModifiedOn,
            SubjectId = @SubjectId
        WHERE SyllabusId = @SyllabusId";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, request);
                    if (rowsAffected > 0)
                    {
                        string selectQuery = "SELECT * FROM tblSyllabus WHERE SyllabusId = @SyllabusId";
                        var data = await _connection.QuerySingleOrDefaultAsync<Syllabus>(selectQuery, new { request.SyllabusId });

                        if (data != null)
                        {
                            string selectQuery1 = "SELECT * FROM tblSyllabusSubjects WHERE SyllabusId = @SyllabusId";
                            var data1 = await _connection.QuerySingleOrDefaultAsync<SyllabusSubject>(selectQuery1, new { request.SyllabusId });
                            if (data1 != null)
                            {
                                string updateSySub = @"
        UPDATE tblSyllabusSubjects 
        SET SyllabusID = @SyllabusID,
            SubjectID = @SubjectID,
            Status = @Status,
            CreatedBy = @CreatedBy,
            CreatedDate = @CreatedDate,
            ModifiedBy = @ModifiedBy,
            ModifiedDate = @ModifiedDate
        WHERE SyllabusSubjectID = @SyllabusSubjectID";

                                var syllabusSubject = new SyllabusSubject
                                {
                                    SyllabusSubjectID = data1.SyllabusSubjectID,
                                    CreatedBy = data.CreatedBy,
                                    CreatedDate = data.CreatedOn,
                                    ModifiedBy = data.ModifiedBy,
                                    ModifiedDate = data.ModifiedOn,
                                    Status = true,
                                    SubjectID = data.SubjectId,
                                    SyllabusID = data.SyllabusId,
                                };
                                int rowsAffected1 = await _connection.ExecuteAsync(updateSySub, syllabusSubject);
                                if (rowsAffected1 > 0)
                                {
                                    return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Updated Successfully", 200);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                                }
                            }
                            else
                            {
                                string insertSubSyQuery = @"
            INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
            VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

                                var syllabusSubject = new SyllabusSubject
                                {
                                    CreatedBy = request.CreatedBy,
                                    CreatedDate = request.CreatedOn,
                                    ModifiedBy = request.ModifiedBy,
                                    ModifiedDate = request.ModifiedOn,
                                    Status = true,
                                    SubjectID = request.SubjectId,
                                    SyllabusID = request.SyllabusId,
                                };
                                int rowsAffected1 = await _connection.ExecuteAsync(insertSubSyQuery, syllabusSubject);
                                if (rowsAffected1 > 0)
                                {
                                    return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Added Successfully", 200);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                                }
                            }
                         
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", "Record not found", 204);
                        }
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

        public async Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request)
        {
            try
            {
                string insertQuery = @"
        INSERT INTO tblSyllabusDetails (SyllabusID, SubjectIndexID, Status, CreatedDate, CreatedBy, ModifiedBy, ModifiedDate, DisplayOrder, IsVerson)
        VALUES (@SyllabusID, @SubjectIndexID, @Status, @CreatedDate, @CreatedBy, @ModifiedBy, @ModifiedDate, @DisplayOrder, @IsVerson)";
                string deleteQuery = @"
        DELETE FROM tblSyllabusDetails
        WHERE SyllabusID = @SyllabusID";

                int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID", new { SyllabusID = request.SyllabusId });
                if (count > 0)
                {
                    int rowsAffected = await _connection.ExecuteAsync(deleteQuery, new { SyllabusID = request.SyllabusId });
                    if (rowsAffected > 0)
                    {
                        if(request.SyllabusDetails != null)
                        {
                            foreach (var data in request.SyllabusDetails)
                            {
                                data.SyllabusID = request.SyllabusId;
                            }
                        }
                        int addedRecords = await _connection.ExecuteAsync(insertQuery, request.SyllabusDetails);

                        if (addedRecords > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Details Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }

                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    if(request.SyllabusDetails != null)
                    foreach (var data in request.SyllabusDetails)
                    {
                        data.SyllabusID = request.SyllabusId;
                    }
                    int addedRecords = await _connection.ExecuteAsync(insertQuery, request.SyllabusDetails);

                    if (addedRecords > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Details Added Successfully", 200);
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
    }
}