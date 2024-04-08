using Dapper;
using System.Data;
using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;

namespace UserManagement_API.Repository.Implementations
{
    public class GenerateLicenseRepository : IGenerateLicenseRepository
    {
        private readonly IDbConnection _connection;

        public GenerateLicenseRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request)
        {
            try
            {
                if (request.GenerateLicenseID == 0)
                {
                    string query = @"
        INSERT INTO tblGenerateLicense (SchoolID, SchoolName, SchoolCode, BranchName, BranchCode, StateName, DistrictName, ChairmanEmail, ChairmanMobile, PrincipalEmail, PrincipalMobile, stateid, DistrictID)
        VALUES (@SchoolID, @SchoolName, @SchoolCode, @BranchName, @BranchCode, @StateName, @DistrictName, @ChairmanEmail, @ChairmanMobile, @PrincipalEmail, @PrincipalMobile, @stateid, @DistrictID)
        SELECT SCOPE_IDENTITY();";
                    var newLicense = new GenerateLicense
                    {
                        BranchCode = request.BranchCode,
                        BranchName = request.BranchName,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        DistrictID = request.DistrictID,
                        DistrictName = request.DistrictName,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        SchoolCode = request.SchoolCode,
                        SchoolID = request.SchoolID,
                        SchoolName = request.SchoolName,
                        stateid = request.stateid,
                        StateName = request.StateName,
                    };
                    var generatedId = await _connection.QueryFirstOrDefaultAsync<int>(query, newLicense);
                    if (generatedId != 0)
                    {
                        string insertQuery = @"
                INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, Validity)
                VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @Validity)";
                        if (request.LicenseDetails != null)
                        {
                            foreach (var item in request.LicenseDetails)
                            {
                                item.GenerateLicenseID = generatedId;
                            }
                        }
                        int rowsAffected = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "License Generated Successfully.", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
        UPDATE tblGenerateLicense
        SET SchoolID = @SchoolID,
            SchoolName = @SchoolName,
            SchoolCode = @SchoolCode,
            BranchName = @BranchName,
            BranchCode = @BranchCode,
            StateName = @StateName,
            DistrictName = @DistrictName,
            ChairmanEmail = @ChairmanEmail,
            ChairmanMobile = @ChairmanMobile,
            PrincipalEmail = @PrincipalEmail,
            PrincipalMobile = @PrincipalMobile,
            stateid = @stateid,
            DistrictID = @DistrictID
        WHERE GenerateLicenseID = @GenerateLicenseID;";
                    var newLicense = new GenerateLicense
                    {
                        BranchCode = request.BranchCode,
                        BranchName = request.BranchName,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        DistrictID = request.DistrictID,
                        DistrictName = request.DistrictName,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        SchoolCode = request.SchoolCode,
                        SchoolID = request.SchoolID,
                        SchoolName = request.SchoolName,
                        stateid = request.stateid,
                        StateName = request.StateName,
                        GenerateLicenseID = request.GenerateLicenseID,
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newLicense);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblLicenseDetail WHERE GenerateLicenseID = @GenerateLicenseID", new { request.GenerateLicenseID });
                        if (count > 0)
                        {
                            string deleteQuery = @"
        DELETE FROM tblLicenseDetail
        WHERE GenerateLicenseID = @GenerateLicenseID";
                            int rowsAffected1 = await _connection.ExecuteAsync(deleteQuery, new { request.GenerateLicenseID });
                            if (rowsAffected1 > 0)
                            {
                                string insertQuery = @"
                INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, Validity)
                VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @Validity)";
                                if (request.LicenseDetails != null)
                                    foreach (var item in request.LicenseDetails)
                                    {
                                        item.GenerateLicenseID = request.GenerateLicenseID;
                                    }
                                int addedRecords = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                                if (addedRecords > 0)
                                {
                                    return new ServiceResponse<string>(true, "Operation Successful", "License Details Added Successfully", 200);
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
                            string insertQuery = @"
                INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, Validity)
                VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @Validity)";
                            if (request.LicenseDetails != null)
                                foreach (var item in request.LicenseDetails)
                                {
                                    item.GenerateLicenseID = request.GenerateLicenseID;
                                }
                            int addedRecords = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                            if (addedRecords > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "License Details Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                            }
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

        public async Task<ServiceResponse<GenerateLicenseDTO>> GetGenerateLicenseById(int GenerateLicenseID)
        {
            try
            {
                GenerateLicenseDTO response = new GenerateLicenseDTO();
                string selectQuery = @"
        SELECT * 
        FROM tblGenerateLicense
        WHERE GenerateLicenseID = @GenerateLicenseID;";
                var generateLicense = await _connection.QueryFirstOrDefaultAsync<GenerateLicense>(selectQuery, new { GenerateLicenseID });
                if (generateLicense != null)
                {
                    response.GenerateLicenseID = generateLicense.GenerateLicenseID;
                    response.SchoolID = generateLicense.SchoolID;
                    response.SchoolName = generateLicense.SchoolName;
                    response.SchoolCode = generateLicense.SchoolCode;
                    response.BranchName = generateLicense.BranchName;
                    response.BranchCode = generateLicense.BranchCode;
                    response.StateName = generateLicense.StateName;
                    response.DistrictName = generateLicense.DistrictName;
                    response.ChairmanEmail = generateLicense.ChairmanEmail;
                    response.ChairmanMobile = generateLicense.ChairmanMobile;
                    response.PrincipalEmail = generateLicense.PrincipalEmail;
                    response.PrincipalMobile = generateLicense.PrincipalMobile;
                    response.stateid = generateLicense.stateid;
                    response.DistrictID = generateLicense.DistrictID;

                    string query = @"
        SELECT * 
        FROM tblLicenseDetail
        WHERE GenerateLicenseID = @GenerateLicenseID;";
                    var data = await _connection.QueryAsync<LicenseDetail>(query, new { GenerateLicenseID });
                    if (data != null)
                    {
                        response.LicenseDetails = data.AsList();
                    }
                    else
                    {
                        response.LicenseDetails = [];
                    }
                    return new ServiceResponse<GenerateLicenseDTO>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<GenerateLicenseDTO>(false, "Record not found", new GenerateLicenseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateLicenseDTO>(false, ex.Message, new GenerateLicenseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateLicenseListDTO>>> GetGenerateLicenseList()
        {
            try
            {
                List<GenerateLicenseListDTO> resposne = [];
                string selectQuery = @"SELECT * FROM tblGenerateLicense";
                var data = await _connection.QueryAsync<GenerateLicense>(selectQuery);
                if(data != null)
                {
                    foreach(var item in data)
                    {
                        var record = new GenerateLicenseListDTO
                        {
                            GenerateLicenseID = item.GenerateLicenseID,
                            SchoolCode = item.SchoolCode,
                            SchoolName = item.SchoolName
                        };
                        resposne.Add(record);
                    }
                    return new ServiceResponse<List<GenerateLicenseListDTO>>(true, "Records found", resposne, 500);
                }
                else
                {
                    return new ServiceResponse<List<GenerateLicenseListDTO>>(false, "Records not found", new List<GenerateLicenseListDTO>(), 500);
                }
            }
            catch(Exception ex)
            {
                return new ServiceResponse<List<GenerateLicenseListDTO>>(false, ex.Message, new List<GenerateLicenseListDTO>(), 500);
            }
        }
    }
}
