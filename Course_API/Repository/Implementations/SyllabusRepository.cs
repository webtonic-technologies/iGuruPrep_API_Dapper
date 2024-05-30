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
        public async Task<ServiceResponse<string>> AddUpdateSyllabus(SyllabusDTO request)
        {
            try
            {
                if (request.SyllabusId == 0)
                {
                    string insertQuery = @"
                INSERT INTO Syllabus (BoardID, CourseId, ClassId, SyllabusName, Status, createdby, createdon, modifiedby, modifiedon, SubjectId, APID, empid, villagename, DesignationName, RoleName, boardname, classname, coursename, subjectname, APname, EmployeeID, EmpFirstName)
                VALUES (@BoardID, @CourseId, @ClassId, @SyllabusName, @Status, @createdby, @createdon, @modifiedby, @modifiedon, @SubjectId, @APID, @empid, @villagename, @DesignationName, @RoleName, @boardname, @classname, @coursename, @subjectname, @APname, @EmployeeID, @EmpFirstName);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var syllabusId = await _connection.ExecuteScalarAsync<int>(insertQuery, request);
                    if (syllabusId != 0)
                    {
                        int rowsAffected = SyllabusSubjectMapping(request.SyllabusSubjects ??= ([]), syllabusId);
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
                UPDATE Syllabus
                SET 
                    BoardID = @BoardID,
                    CourseId = @CourseId,
                    ClassId = @ClassId,
                    SyllabusName = @SyllabusName,
                    Status = @Status,
                    modifiedby = @modifiedby,
                    modifiedon = @modifiedon,
                    SubjectId = @SubjectId,
                    APID = @APID,
                    empid = @empid,
                    villagename = @villagename,
                    DesignationName = @DesignationName,
                    RoleName = @RoleName,
                    boardname = @boardname,
                    classname = @classname,
                    coursename = @coursename,
                    subjectname = @subjectname,
                    APname = @APname,
                    EmployeeID = @EmployeeID,
                    EmpFirstName = @EmpFirstName
                WHERE SyllabusId = @SyllabusId;";
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, request);
                    if (rowsAffected > 0)
                    {
                        string selectQuery = "SELECT * FROM tblSyllabus WHERE SyllabusId = @SyllabusId";
                        var data = await _connection.QuerySingleOrDefaultAsync<Syllabus>(selectQuery, new { request.SyllabusId });

                        if (data != null)
                        {
                            int Affectedrows = SyllabusSubjectMapping(request.SyllabusSubjects ??= ([]), request.SyllabusId);
                            if (Affectedrows > 0)
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
                string insertQuery = @"INSERT INTO tblSyllabusDetails (SyllabusID, SubjectIndexID, Status, IsVerson)
                                    VALUES (@SyllabusID, @SubjectIndexID, @Status, @IsVerson)";
                string deleteQuery = @"DELETE FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID";

                int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID", new { SyllabusID = request.SyllabusId });
                if (count > 0)
                {
                    int rowsAffected = await _connection.ExecuteAsync(deleteQuery, new { SyllabusID = request.SyllabusId });
                    if (rowsAffected > 0)
                    {
                        if (request.SyllabusDetails != null)
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
                    if (request.SyllabusDetails != null)
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
        private int SyllabusSubjectMapping(List<SyllabusSubject> request, int SyllabusId)
        {
            foreach (var data in request)
            {
                data.SyllabusID = SyllabusId;
            }
            string query = "SELECT COUNT(*) FROM [tblSyllabusSubjects] WHERE [SyllabusID] = @SyllabusID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SyllabusID = SyllabusId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSyllabusSubjects] WHERE [SyllabusID] = @SyllabusID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SyllabusID = SyllabusId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
    }
}